using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetTopologySuite.Geometries;
using GeoAPI.Geometries;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;

namespace gsec.model
{
    public class Ranger : IDisplayableGeoElement
    {
        // Database fields
        public virtual long ID { get; set; }
        public virtual string Name { get; set; }
        public virtual RangerState State { get; set; }
        public virtual Point Position { get; set; }
        public virtual LineString Route { get; set; }

        public Graphic Graphic { get; set; }
        public Graphic RangeGraphic { get; set; }
        
        public static int Speed { get; set; } = 72;

        public MapPoint EsriPosition => Position.ToEsriPoint();

        

        public override bool Equals(object obj)
        {
            var ranger = obj as Ranger;
            return ranger != null &&
                   ID == ranger.ID;
        }

        public override int GetHashCode()
        {
            return 1213502048 + ID.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Ranger{0} ({1:0.###}, {2:0.###})", ID, Position.X, Position.Y);
        }
    }
}
