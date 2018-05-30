using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using gsec.model;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;

using gsec.routing;
using gsec.ui.layers;
using Esri.ArcGISRuntime.Data;
using gsec.ui.events;
using gsec.ui.animations;

namespace gsec.ui
{
    public enum BasemapChoice
    {
        None,
        OpenStreetMap,
        NationalGeographic,
        StreetsVector
    }

    public enum PursuitPolicy
    {
        FIRST_FREE,
        NEAREST,
        COMBINED,
    }

    public class ViewModel
    {
        public Model Model { get; private set; }
        public Dictionary<BasemapChoice, Func<Basemap>> BasemapDict { get; set; }
        public GraphicsOverlay EditOverlay { get; private set; }
        public GraphicsOverlay EditSelectionOverlay { get; private set; }
        public GraphicsOverlay RouteOverlay { get; private set; } // TODO must be removed

        public InterloperLayer InterloperLayer { get; }
        public CrossingLayer CrossingLayer { get; }
        public RangerLayer RangerLayer { get; }
        public RoadLayer RoadLayer { get; }
        public SensorLayer SensorLayer { get; }

        public Map Map { get; set; }
        
        public event RangerSelectionEventHandler RangerSelected;
        public event SensorSelectionEventHandler SensorSelected;
        public Action<string> ShowMessage = null;
        
        public Dictionary<string, IRoutingAlgo> RoutingAlgorithms { get; }
        public IRoutingAlgo Routing { get; set; }

        public PursuitPolicy Policy { get; set; } = PursuitPolicy.NEAREST;

        public Stat Stats { get; set; }

        public Simulation Simulation { get; set; }

        public int RangerSpeed
        {
            get { return Ranger.Speed; }
            set { Ranger.Speed = value; }
        }
        public int InterloperSpeed
        {
            get { return Interloper.Speed; }
            set { Interloper.Speed = value; }
        }
        public int SensorRange
        {
            get { return Sensor.Range; }
            set { Sensor.Range = value; SensorLayer.UpdateRanges(); }
        }
        public int TimeScale
        {
            get { return Simulation.TimeScale; }
            set { Simulation.TimeScale = value; }
        }

        public IDisplayableGeoElement GeoElementContext { get; set; }

        private EditModeType? editMode;

        public EditModeType? GetEditMode()
        {
            return editMode;
        }

        public void SetEditMode(EditModeType? editMode)
        {
            this.editMode = editMode;
            EditOverlay.IsVisible = (editMode != null);
            EditSelectionOverlay.Graphics.Clear(); // clear all selections???
            if (editMode == null)
            {
                GeoElementContext = null;
            }
        }

        private static readonly ViewModel instance = new ViewModel();
        public static ViewModel Instance { get => instance; }

        static ViewModel() { }
        private ViewModel()
        {
            BasemapDict = new Dictionary<BasemapChoice, Func<Basemap>>()
            {
                { BasemapChoice.None, () => new Basemap() }, // TODO can't drag over plain basemap. would be good to fix it
                { BasemapChoice.OpenStreetMap, Basemap.CreateOpenStreetMap },
                { BasemapChoice.NationalGeographic, Basemap.CreateNationalGeographic },
                { BasemapChoice.StreetsVector, Basemap.CreateStreetsVector },
            };
            
            Model = new Model();
            Model.Load();
            
            RoadLayer = new RoadLayer(Model.Roads);
            CrossingLayer = new CrossingLayer(Model.Crossings);
            SensorLayer = new SensorLayer(Model.Sensors);
            RangerLayer = new RangerLayer(Model.Rangers);
            InterloperLayer = new InterloperLayer(Model.Interlopers);

            findCrossingsLeafs();
            
            RoutingAlgorithms = new Dictionary<string, IRoutingAlgo>()
            {
                { "Dijkstra (pgRouting)", new PgRoutingDijkstra() },
                { "A* (pgRouting)", new PgRoutingAStar() },
            };

            EditOverlay = createEditOverlay();
            EditSelectionOverlay = new GraphicsOverlay();
            RouteOverlay = new GraphicsOverlay();

            Simulation = new Simulation(this);

            Stats = new Stat();
            Stats.NrCaught = 0;
            Stats.NrEscaped = 0;
            Stats.NrInterlopers = InterloperLayer.Elements.Count();
            Stats.NrRangers = RangerLayer.Elements.Count();
            Stats.NrSensors = SensorLayer.Elements.Count();
            Stats.NrAlarms = 0;
        }

        private void findCrossingsLeafs()
        {
            List<Crossing> leafs = new List<Crossing>();

            foreach (var crossing in CrossingLayer.Elements)
            {
                IEnumerable<Road> elements = RoadLayer.Elements.Where(r => GeometryEngine.Intersects(r.Graphic.Geometry, crossing.Graphic.Geometry));
                if (elements.Count() == 1)
                    leafs.Add(crossing);
            }

            CrossingLayer.Leafs = leafs;
        }

        private GraphicsOverlay createEditOverlay()
        {
            GraphicsOverlay overlay = new GraphicsOverlay();
            Graphic overlayGraphic = new Graphic(new Envelope(-180, -90, 180, 90, SpatialReferences.Wgs84), GeneralRenderers.OverlaySymbol);
            overlay.Graphics.Add(overlayGraphic);
            overlay.IsVisible = false;
            overlay.Opacity = 0.5;
            editMode = null;
            return overlay;
        }
        
        public Map GetBasemap(BasemapChoice basemap)
        {
            Map map = new Map(BasemapDict[basemap]());

            // lock to roads extent?
            //map.MinScale = 80000;
            //map.MaxScale = 4000;

            return map;
        }

        public Envelope GetExtent()
        {
            EnvelopeBuilder envelope = new EnvelopeBuilder(SpatialReferences.Wgs84);
            List<Graphic> graphics = RoadLayer.GetBaseGraphics();

            foreach (var graphic in graphics)
            {
                envelope.UnionOf(graphic.Geometry.Extent);
            }

            envelope.Expand(1.3);
            return envelope.Extent;
        }

        public void HandleEditModeMouseMove(MapPoint location)
        {
            switch (editMode.Value)
            {
                case EditModeType.RangerAdd:
                    Log(Messages.SELECT_LOCATION);
                    SnapToNearestPointInLayer(location, RoadLayer);
                    break;
                case EditModeType.RangerDel:
                    Log(Messages.SELECT_RANGER);
                    SnapToNearestPointInLayer(location, RangerLayer);
                    break;
                case EditModeType.SensorAdd:
                    Log(Messages.SELECT_LOCATION);
                    SnapToNearestPointInLayer(location, RoadLayer);
                    break;
                case EditModeType.SensorDel:
                    Log(Messages.SELECT_SENSOR);
                    SnapToNearestPointInLayer(location, SensorLayer);
                    break;
                case EditModeType.InterloperAdd:
                    Log(Messages.SELECT_LOCATION);
                    SnapToNearestPointInLayer(location, CrossingLayer);
                    break;
                case EditModeType.InterloperDel:
                    Log(Messages.SELECT_INTERLOPER);
                    SnapToNearestPointInLayer(location, InterloperLayer);
                    break;
                case EditModeType.RangerDriveTo:
                    Log(Messages.SELECT_DESTINATION);
                    SnapToNearestPointInLayer(location, CrossingLayer);
                    break;
                case EditModeType.InterloperDriveTo:
                    Log(Messages.SELECT_DESTINATION);
                    SnapToNearestPointInLayer(location, CrossingLayer);
                    break;
                default:
                    // nothing to do for me
                    break;
            }
        }

        public void HandleEditModeGeoTap()
        {
            switch (editMode.Value)
            {
                case EditModeType.RangerAdd:
                    AddRangerAtSelectedLocation();
                    break;
                case EditModeType.RangerDel:
                    RemoveRangerAtSelectedLocation();
                    break;
                case EditModeType.SensorAdd:
                    AddSensorAtSelectedLocation();
                    break;
                case EditModeType.SensorDel:
                    RemoveSensorAtSelectedLocation();
                    break;
                case EditModeType.InterloperDel:
                    RemoveInterloperAtSelectedLocation();
                    break;
                case EditModeType.RangerDriveTo:
                    LeadMobileToSelectedLocation(GeoElementContext as Ranger);
                    break;
                case EditModeType.InterloperDriveTo:
                    LeadMobileToSelectedLocation(GeoElementContext as Interloper);
                    break;
                case EditModeType.InterloperAdd:
                    AddInterloperAtSelectedLocation();
                    SetEditMode(EditModeType.InterloperDriveTo);
                    return; // no break here as we continue with different edit mode. i know...
                default:
                    throw new GsecException("Unknown event: " + editMode.Value);
            }

            // handled. stop edit mode
            SetEditMode(null);
        }
        
        public void HandleIdentifyResults(IReadOnlyList<IdentifyGraphicsOverlayResult> identifyResults)
        {
            foreach (IdentifyGraphicsOverlayResult result in identifyResults)
            {
                if (result.Graphics.Count == 0)
                    continue;

                if (result.GraphicsOverlay == RangerLayer.GetOverlay())
                {
                    Ranger ranger = RangerLayer.ByGraphic(result.Graphics.First());
                    GeoElementContext = ranger;
                    OnRangerSelected(ranger);
                    return;
                }
                else if (result.GraphicsOverlay == SensorLayer.GetOverlay())
                {
                    Sensor sensor = SensorLayer.ByGraphic(result.Graphics[0]);
                    GeoElementContext = sensor;
                    OnSensorSelected(sensor);
                    return;
                }
            }
            
            Console.WriteLine("nothing here");
        }
        
        public void OnRangerSelected(Ranger ranger)
        {
            RangerSelected?.Invoke(this, new RangerSelectionEventArgs(ranger));
        }

        public void OnSensorSelected(Sensor sensor)
        {
            SensorSelected?.Invoke(this, new SensorSelectionEventArgs(sensor));
        }
        
        public void Log(string fmt, params object[] args)
        {
            string msg = string.Format(fmt, args);
            ShowMessage?.Invoke(msg);
        }

        public void SnapToNearestPointInLayer<T>(MapPoint mapPoint, AbstractLayer<T> layer) where T : IDisplayableGeoElement
        {
            MapPoint nearest = GeoUtil.GetNearestCoordinateInGraphicsCollection(mapPoint, layer.GetBaseGraphics());

            EditSelectionOverlay.ClearSelection();
            EditSelectionOverlay.Graphics.Clear();

            Graphic graphic = new Graphic(nearest, GeneralRenderers.NearestPointSymbol);
            graphic.IsSelected = true;
            EditSelectionOverlay.Graphics.Add(graphic);
        }

        public MapPoint GetRandomPointInLayer<T>(AbstractLayer<T> layer) where T : IDisplayableGeoElement
        {
            IList<Graphic> graphics = layer.GetBaseGraphics();
            return GeoUtil.GetRandomPointInGraphicsCollection(graphics);
        }

        public void ClearAllSelections()
        {
            RangerLayer.GetOverlay().ClearSelection();
            RoadLayer.GetOverlay().ClearSelection();
            CrossingLayer.GetOverlay().ClearSelection();
            SensorLayer.GetOverlay().ClearSelection();
            InterloperLayer.GetOverlay().ClearSelection();

            EditSelectionOverlay.ClearSelection();
            EditSelectionOverlay.Graphics.Clear();

            RouteOverlay.ClearSelection();
            RouteOverlay.Graphics.Clear();
        }

        public void StartSimulation()
        {
            Simulation.Start();
            Log(Messages.SIM_STARTED);
        }

        public void StopSimulation()
        {
            Simulation.Stop();
            Log(Messages.SIM_STOPPED);
        }

        public T AddRandomElement<T, K>(AbstractLayer<T> targetLayer, AbstractLayer<K> randomLayer)
            where T : IDisplayableGeoElement where K : IDisplayableGeoElement
        {
            MapPoint randomLocation = GetRandomPointInLayer(randomLayer);
            T elem = targetLayer.AddElement(randomLocation);
            return elem;
        }

        public void AddRangerRandom()
        {
            AddRandomElement(RangerLayer, RoadLayer);
            Log(Messages.RANGER_ADDED);

            Stats.NrRangers++;
        }

        public void AddSensorRandom()
        {
            AddRandomElement(SensorLayer, RoadLayer);
            Log(Messages.SENSOR_ADDED);
            
            Stats.NrSensors++;
        }

        public void AddInterloperRandom()
        {
            Crossing src, dst;
            Random random = new Random();

            do
            {
                src = CrossingLayer.Leafs[random.Next(0, CrossingLayer.Leafs.Count)];
                dst = CrossingLayer.Leafs[random.Next(0, CrossingLayer.Leafs.Count)];
            } while (src == dst);

            Interloper interloper = InterloperLayer.AddElement(src.Graphic.Geometry as MapPoint);
            SingleRoute route = Routing.GetRouteFromCrossing(null, src, dst);
            interloper.Route = route;
            route.Create();
            route.Graphic = new Graphic(route.Geom.ToEsriPolyline(), GeneralRenderers.RouteSymbol);
            interloper.Update();

            Log(Messages.INTERLOPER_ADDED);

            Stats.NrInterlopers++;
        }

        public void AddRangerAtSelectedLocation()
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            RangerLayer.AddElement(location.ToWgs84());

            Log(Messages.RANGER_ADDED);

            Stats.NrRangers++;
        }

        public void AddSensorAtSelectedLocation()
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            SensorLayer.AddElement(location.ToWgs84());

            Log(Messages.SENSOR_ADDED);

            Stats.NrSensors++;
        }
        
        public void AddInterloperAtSelectedLocation()
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            Interloper interloper = InterloperLayer.AddElement(location.ToWgs84());
            GeoElementContext = interloper;
            // XXXXXXXXXXXXXXXXXXX find route

            Log(Messages.INTERLOPER_ADDED);

            Stats.NrInterlopers++;
        }

        public void RemoveRangerAtSelectedLocation()
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            Ranger ranger = RangerLayer.ByPosition(location.ToWgs84());
            RangerLayer.RemoveElement(ranger);

            Log(Messages.RANGERS_REMOVED);
        }

        public void RemoveSensorAtSelectedLocation()
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            Sensor sensor = SensorLayer.ByPosition(location.ToWgs84());
            SensorLayer.RemoveElement(sensor);

            Log(Messages.SENSORS_REMOVED);
        }

        public void RemoveInterloperAtSelectedLocation()
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            Interloper interloper = InterloperLayer.ByPosition(location.ToWgs84());
            InterloperLayer.RemoveElement(interloper);

            Log(Messages.INTERLOPERS_REMOVED);
        }

        public void RemoveAllInterlopers()
        {
            InterloperLayer.RemoveAll();
            Log(Messages.INTERLOPERS_REMOVED);
        }

        public void RemoveAllRangers()
        {
            RangerLayer.RemoveAll();
            Log(Messages.RANGERS_REMOVED);
        }

        public void RemoveAllSensors()
        {
            SensorLayer.RemoveAll();
            Log(Messages.SENSORS_REMOVED);
        }

        public void LeadMobileToCrossing(MobileUnit unit, Crossing dst)
        {
            SingleRoute route;

            unit.UpdateDbPosition();

            Crossing crossing = CrossingLayer.ByPosition(unit.Graphic.Geometry as MapPoint, 10);
            if (crossing != null)
            {
                route = Routing.GetRouteFromCrossing(unit, crossing, dst);
            }
            else
            {
                Road currentRoad = RoadLayer.ByPosition(unit.Graphic.Geometry as MapPoint, 3);
                if (currentRoad != null)
                {
                    route = Routing.GetRouteFromMidRoad(unit, currentRoad, dst);
                }
                else
                {
                    route = null;
                    Console.WriteLine("SOMETHING WROOOOONG");
                }
            }

            if (route == null)
            {
                Console.WriteLine("NO ROUTE HACK!");
                return;
            }

            unit.Route = route;
            unit.Route.Create();
            unit.Update();

            NewRouteAnimation anim = new NewRouteAnimation(unit.Route, null);
            anim.Start();
            
            // TODO UNCOMMENT
            // testing only
            /*Polyline geom = route.Geom.ToEsriPolyline();
            Graphic g = new Graphic(geom, GeneralRenderers.RouteSymbol);
            RouteOverlay.Graphics.Add(g);
            g.IsSelected = true;
            */

            Log(Messages.NEW_ROUTE);
        }

        public void LeadMobileToSelectedLocation(MobileUnit unit)
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            Crossing destination = CrossingLayer.ByPosition(location);

            LeadMobileToCrossing(unit, destination);
            GeoElementContext = null;
        }

        public void TestRouting()
        {
            int i = 0;

            List<Crossing> crossings = CrossingLayer.Elements;
            Random random = new Random();
            PgRoutingAStar astar = new PgRoutingAStar();
            PgRoutingDijkstra dijkstra = new PgRoutingDijkstra();

            while (i < 1024)
            {
                Crossing c1 = crossings[random.Next(0, crossings.Count)];
                Crossing c2 = crossings[random.Next(0, crossings.Count)];
                if (c1 == c2)
                    continue;

                SingleRoute rd = dijkstra.GetRouteFromCrossing(null, c1, c2);
                SingleRoute ra = astar.GetRouteFromCrossing(null, c1, c2);
                
                if (rd.Length != ra.Length)
                {
                    Graphic gd = new Graphic(rd.Geom.ToEsriPolyline(), GeneralRenderers.RouteSymbol);
                    RouteOverlay.Graphics.Add(gd);

                    Graphic ga = new Graphic(rd.Geom.ToEsriPolyline(), GeneralRenderers.AltRouteSymbol);
                    RouteOverlay.Graphics.Add(ga);
                    Console.WriteLine("found difference. crossings {0} {1}", c1.ID, c2.ID);
                    break;
                }

                i++;
            }

            Console.WriteLine("TestRouting finished");
        }
    }
}
