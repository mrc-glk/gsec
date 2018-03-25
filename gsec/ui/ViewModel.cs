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

namespace gsec.ui
{
    public enum BasemapChoice
    {
        None,
        OpenStreetMap,
        NationalGeographic,
        StreetsVector
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

        public Dictionary<string, Func<Ranger, Road, Crossing, SingleRoute>> RoutingAlgorithms { get; }
        public Func<Ranger, Road, Crossing, SingleRoute> CalculateRoute { get; set; }
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
            set { Sensor.Range = value; }
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

        public ViewModel()
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

            RoutingAlgorithms = new Dictionary<string, Func<Ranger, Road, Crossing, SingleRoute>>()
            {
                { "Dijkstra (pgRouting)", PgRoutingDijkstra.Calculate },
            };

            EditOverlay = createEditOverlay();
            EditSelectionOverlay = new GraphicsOverlay();
            RouteOverlay = new GraphicsOverlay();

            Simulation = new Simulation(this);
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
                    SnapToNearestPointInLayer(location, RoadLayer);
                    break;
                case EditModeType.RangerDel:
                    SnapToNearestPointInLayer(location, RangerLayer);
                    break;
                case EditModeType.SensorAdd:
                    SnapToNearestPointInLayer(location, RoadLayer);
                    break;
                case EditModeType.SensorDel:
                    SnapToNearestPointInLayer(location, SensorLayer);
                    break;
                case EditModeType.InterloperAdd:
                    SnapToNearestPointInLayer(location, RoadLayer);
                    break;
                case EditModeType.InterloperDel:
                    SnapToNearestPointInLayer(location, InterloperLayer);
                    break;
                case EditModeType.RangerDriveTo:
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
                case EditModeType.InterloperAdd:
                    AddInterloperAtSelectedLocation();
                    break;
                case EditModeType.InterloperDel:
                    RemoveInterloperAtSelectedLocation();
                    break;
                case EditModeType.RangerDriveTo:
                    LeadRangerToSelectedLocation(GeoElementContext as Ranger);
                    break;
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

        public void AddRandomElement<T,K>(AbstractLayer<T> targetLayer, AbstractLayer<K> randomLayer)
            where T : IDisplayableGeoElement where K : IDisplayableGeoElement
        {
            MapPoint randomLocation = GetRandomPointInLayer(randomLayer);
            targetLayer.AddElement(randomLocation);
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
        }

        public void StopSimulation()
        {
            Simulation.Stop();
        }

        public void AddRangerAtSelectedLocation()
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            RangerLayer.AddElement(location.ToWgs84());
        }

        public void AddSensorAtSelectedLocation()
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            SensorLayer.AddElement(location.ToWgs84());
        }
        
        public void AddInterloperAtSelectedLocation()
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            InterloperLayer.AddElement(location.ToWgs84());
        }

        public void RemoveRangerAtSelectedLocation()
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            Ranger ranger = RangerLayer.ByPosition(location.ToWgs84());
            RangerLayer.RemoveElement(ranger);
        }

        public void RemoveSensorAtSelectedLocation()
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            Sensor sensor = SensorLayer.ByPosition(location.ToWgs84());
            SensorLayer.RemoveElement(sensor);
        }

        public void RemoveInterloperAtSelectedLocation()
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            Interloper interloper = InterloperLayer.ByPosition(location.ToWgs84());
            InterloperLayer.RemoveElement(interloper);
        }

        public void LeadRangerToCrossing(Ranger ranger, Crossing destination)
        {
            Road currentRoad = RoadLayer.ByPosition(ranger.Graphic.Geometry as MapPoint); // what if ranger is on roads crossing?
            
            SingleRoute route = CalculateRoute(ranger, currentRoad, destination);

            // testing only
            Polyline geom = route.Geom.ToEsriPolyline();
            Graphic g = new Graphic(geom, GeneralRenderers.RouteSymbol);
            RouteOverlay.Graphics.Add(g);
            g.IsSelected = true;
        }

        public void LeadRangerToSelectedLocation(Ranger ranger)
        {
            MapPoint location = EditSelectionOverlay.SelectedGraphics.First().Geometry as MapPoint;
            Crossing destination = CrossingLayer.ByPosition(location);

            LeadRangerToCrossing(ranger, destination);

            GeoElementContext = null;
        }
    }
}
