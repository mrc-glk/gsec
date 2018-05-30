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
        // it's needed as a storage for ESRI geometry (graphic itself is a side-effect here)
        Graphic Graphic { get; set; }
        long ID { get; set; }
    }
}
