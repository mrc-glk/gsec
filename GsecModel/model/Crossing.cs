using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using NetTopologySuite.Geometries;

namespace gsec.model
{
    public class Crossing : IDisplayableGeoElement
    {
        public virtual long ID { get; set; }
        public virtual Point Position { get; set; }

        public Graphic Graphic { get; set; }

        public Esri.ArcGISRuntime.Geometry.Geometry DoNotUseThisGeometry => Position.ToEsriPoint();

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
    }
}
