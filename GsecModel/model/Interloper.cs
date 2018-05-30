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
    public class Interloper : MobileUnit, IDisplayableGeoElement
    {
        public override long ID { get; set; }
        public override Point Position { get; set; }

        //public static int Speed { get; set; } = 36; XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        public static int Speed { get; set; } = 72;

        public override Graphic Graphic { get; set; }
        
        public MapPoint EsriPosition { get { return Graphic.Geometry as MapPoint; } }

        public Sensor RaisedAlarm { get; set; }

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

        public override void Create()
        {
            InterloperManager.Instance.Create(this);
        }

        public override void Update()
        {
            InterloperManager.Instance.Update(this);
        }

        public override void Delete()
        {
            //Route = null;
            InterloperManager.Instance.Delete(this);
        }
    }
}
