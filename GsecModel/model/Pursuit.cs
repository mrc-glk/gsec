using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.UI;
using gsec.model.managers;

namespace gsec.model
{
    public class Pursuit : IDisplayableGeoElement
    {
        public virtual long ID { get; set; }
        public virtual Ranger Ranger { get; set; }
        public virtual Interloper Interloper { get; set; }
        public Graphic Graphic { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
