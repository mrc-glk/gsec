using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using gsec.model.managers;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model
{
    public enum MobileUnitState
    {
        FREE,
        EN_ROUTE,
        FINISHED,
    }

    public abstract class MobileUnit : IDisplayableGeoElement, IPersistable
    {
        public abstract Graphic Graphic { get; set; }
        public abstract long ID { get; set; }
        public abstract Point Position { get; set; }

        public MobileUnitState State { get; set; }

        protected SingleRoute route;
        public SingleRoute Route
        {
            get { return route; }
            set
            {
                setRouteInternal(value);
            }
        }

        private void setRouteInternal(SingleRoute newRoute)
        {
            if (newRoute != null)
            {
                route = newRoute;
                route.Graphic = new Graphic();
                route.Graphic.Geometry = route.Geom.ToEsriPolyline();
                
                State = MobileUnitState.EN_ROUTE;
            }
            else
            {
                if (route != null)
                {
                    route.Delete();
                    route = null;
                }
                State = MobileUnitState.FREE;
            }
        }

        public MapPoint UpdateUiPosition(double speedMPS, double seconds)
        {
            SingleRoute route = Route;
            if (route == null)
            {
                return Graphic.Geometry as MapPoint;
            }
            
            double partDistPerc = speedMPS * seconds / route.Length;
            route.Progress += partDistPerc;

            if (route.Progress > 1.0)
            {
                Console.WriteLine("{0} finished route", this.ToString());
                route.Progress = 1.0;
                State = MobileUnitState.FINISHED;
            }

            double sumDistDegrees = route.Progress * route.Length * GeoUtil.TEMP_METER_TO_DEGREE;
            MapPoint newPos = GeometryEngine.CreatePointAlong(route.Graphic.Geometry as Polyline, sumDistDegrees);
            Graphic.Geometry = newPos;

            return newPos;
        }

        public void UpdateDbPosition()
        {
            MapPoint mp = Graphic.Geometry as MapPoint;
            Position = mp.ToNtsPoint();
                        
            //if (Route != null)
            //    Route.Update();
            this.Update();
        }

        public abstract void Create();
        public abstract void Update();
        public abstract void Delete();
    }
}

