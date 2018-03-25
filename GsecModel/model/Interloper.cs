using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model
{
    public class Interloper : IDisplayableGeoElement
    {
        public virtual long ID { get; set; }
        public virtual Point Position { get; set; }

        public static int Speed { get; set; } = 36;

        public Graphic Graphic { get; set; }
        
        public override bool Equals(object obj)
        {
            var interloper = obj as Interloper;
            return interloper != null &&
                   ID == interloper.ID;
        }

        public override int GetHashCode()
        {
            return 1213502048 + ID.GetHashCode();
        }
    }
}
