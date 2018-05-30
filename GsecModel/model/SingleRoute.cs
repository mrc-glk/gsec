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
    public class SingleRoute : IDisplayableGeoElement, IPersistable
    {
        public virtual long ID { get; set; }
        public virtual double Length { get; set; }
        public virtual LineString Geom { get; set; }
        public virtual double Progress { get; set; }
        
        public Graphic Graphic { get; set; }

        public override bool Equals(object obj)
        {
            var route = obj as SingleRoute;
            return route != null &&
                   ID == route.ID;
        }

        public override int GetHashCode()
        {
            var hashCode = 847463869;
            hashCode = hashCode * -1521134295 + ID.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return "Route(" + ID + ")";
        }

        public void Delete()
        {
            SingleRouteManager.Instance.Delete(this);
        }

        public void Create()
        {
            SingleRouteManager.Instance.Create(this);
        }

        public void Update()
        {
            SingleRouteManager.Instance.Update(this);
        }
    }
}
