using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;

namespace gsec.model
{
    public interface IDisplayableGeoElement
    {
        Graphic Graphic { get; set; }
        long ID { get; set; }
    }
}
