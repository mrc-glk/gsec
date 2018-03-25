using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace gsec.ui
{
    public enum EditModeType
    {
        RangerAdd,
        RangerDel,
        RangerDriveTo,
        SensorAdd,
        SensorDel,
        InterloperAdd,
        InterloperDel,
    }
}
