using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Esri.ArcGISRuntime.Symbology;
using gsec;
using gsec.model;

namespace gsec.ui
{
    public static class GeneralRenderers
    {
        public static SimpleFillSymbol OverlaySymbol { get; set; }
        public static SimpleMarkerSymbol NearestPointSymbol { get; set; }
        public static SimpleLineSymbol RouteSymbol { get; set; }
        public static SimpleLineSymbol RoadSymbol { get; set; }
        public static SimpleMarkerSymbol CrossingSymbol { get; set; }
        public static SimpleMarkerSymbol RangerSymbol { get; set; }
        public static SimpleMarkerSymbol SensorSymbol { get; set; }
        public static SimpleMarkerSymbol InterloperSymbol { get; set; }
        public static SimpleFillSymbol SensorRangeSymbol { get; set; }
        public static SimpleFillSymbol RangerRangeSymbol { get; set; }

        static GeneralRenderers()
        {
            OverlaySymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(255, 192, 128, 128), null);
            NearestPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, Color.FromArgb(255, 255, 0, 255), 10.0);
            RouteSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(255, 192, 128, 0), 2.0);

            RoadSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.FromArgb(255, 128, 32, 180), 3.0);
            CrossingSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.FromArgb(255, 36, 74, 99), 10.0);
            RangerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, Color.FromArgb(255, 0, 255, 0), 10.0);
            SensorSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Color.FromArgb(255, 255, 255, 0), 10.0);
            InterloperSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.FromArgb(255, 0, 128, 128), 10.0);

            SensorRangeSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(255, 128, 255, 128), RoadSymbol);
            RangerRangeSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(255, 128, 128, 255), RoadSymbol);
        }
    }
}
