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
using gsec.model.managers;

namespace gsec.model
{
    public class Ranger : MobileUnit, IDisplayableGeoElement
    {
        // Database fields
        public override long ID { get; set; }
        public virtual string Name { get; set; }
        public virtual RangerState RState { get; set; }
        public override Point Position { get; set; }

        public override Graphic Graphic { get; set; }
        public Graphic RangeGraphic { get; set; }
        public Graphic FineGraphic { get; set; }

        public static int FineRange { get; set; } = 250;
        public static int Speed { get; set; } = 90;

        public MapPoint EsriPosition { get { return Graphic.Geometry as MapPoint;  } }
        
        public Ranger()
        {
            State = MobileUnitState.FREE;
        }

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

        public override void Create()
        {
            RangerManager.Instance.Create(this);
        }

        public override void Update()
        {
            RangerManager.Instance.Update(this);
        }

        public override void Delete()
        {
            Route = null;
            RangerManager.Instance.Delete(this);
        }
    }
}
