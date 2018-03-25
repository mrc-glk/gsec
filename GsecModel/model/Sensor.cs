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
    public class Sensor : IDisplayableGeoElement
    {
        public virtual long ID { get; set; }
        public virtual Point Position { get; set; }

        public static int Range { get; set; } = 20;

        public Graphic Graphic { get; set; }
        public Graphic RangeGraphic { get; set; }
        
        public override bool Equals(object obj)
        {
            var sensor = obj as Sensor;
            return sensor != null &&
                   ID == sensor.ID;
        }

        public override int GetHashCode()
        {
            return 1213502048 + ID.GetHashCode();
        }
    }
}
