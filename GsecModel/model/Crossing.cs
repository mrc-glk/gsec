using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using gsec.model.managers;
using NetTopologySuite.Geometries;

namespace gsec.model
{
    public class Crossing : IDisplayableGeoElement, IPersistable
    {
        public virtual long ID { get; set; }
        public virtual Point Position { get; set; }
        
        public Graphic Graphic { get; set; }
        public MapPoint EsriPosition { get { return Graphic.Geometry as MapPoint; } }
        
        public override bool Equals(object obj)
        {
            var crossing = obj as Crossing;
            return crossing != null &&
                   ID == crossing.ID;
        }

        public override int GetHashCode()
        {
            return 1213502048 + ID.GetHashCode();
        }

        public void Delete()
        {
            CrossingManager.Instance.Delete(this);
        }

        public void Create()
        {
            CrossingManager.Instance.Create(this);
        }

        public void Update()
        {
            CrossingManager.Instance.Update(this);
        }
    }
}
