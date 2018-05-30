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
        public static SimpleLineSymbol AltRouteSymbol { get; set; }
        public static SimpleLineSymbol RoadSymbol { get; set; }
        public static SimpleMarkerSymbol CrossingSymbol { get; set; }
        public static SimpleMarkerSymbol RangerSymbol { get; set; }
        public static SimpleMarkerSymbol SensorSymbol { get; set; }
        public static SimpleMarkerSymbol InterloperSymbol { get; set; }

        public static SimpleFillSymbol SensorRangeFillSymbol { get; set; }
        public static SimpleLineSymbol SensorRangeOutlineSymbol { get; set; }
        public static SimpleFillSymbol SensorAlarmFillSymbol { get; set; }
        public static SimpleLineSymbol SensorAlarmOutlineSymbol { get; set; }

        public static SimpleFillSymbol RangerRangeFillSymbol { get; set; }
        public static SimpleLineSymbol RangerRangeOutlineSymbol { get; set; }

        public static PictureMarkerSymbol RangerPicSymbol { get; set; }
        public static PictureMarkerSymbol SensorPicSymbol { get; set; }
        public static PictureMarkerSymbol InterloperPicSymbol { get; set; }
        public static PictureMarkerSymbol DollarPicSymbol { get; set; }

        static GeneralRenderers()
        {
            OverlaySymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(255, 192, 128, 128), null);
            NearestPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, Color.FromArgb(255, 255, 0, 255), 10.0);
            RouteSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(255, 192, 128, 0), 2.0);
            AltRouteSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(255, 0, 255, 255), 4.0);

            RoadSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.FromArgb(255, 128, 32, 180), 3.0);
            CrossingSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.FromArgb(255, 36, 74, 99), 6.0);
            RangerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, Color.FromArgb(255, 250, 48, 92), 10.0);
            SensorSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Color.FromArgb(255, 255, 255, 0), 10.0);
            InterloperSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, Color.FromArgb(255, 127, 0, 255), 10.0);

            SensorRangeOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.FromArgb(255, 255, 0, 0), 2.0);
            SensorRangeFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.ForwardDiagonal, Color.FromArgb(128, 255, 128, 128), null);

            SensorAlarmOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(128, 255, 0, 0), 1.5);
            SensorAlarmFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(32, 255, 0, 0), SensorAlarmOutlineSymbol);

            RangerRangeOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.FromArgb(255, 0, 0, 255), 2.0);
            RangerRangeFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(255, 128, 128, 255), RangerRangeOutlineSymbol);

            RangerPicSymbol = new PictureMarkerSymbol(new Uri("c://Users//mrc//Desktop//ikony//ranger_32.png")) { Height = 20, Width = 20 };
            SensorPicSymbol = new PictureMarkerSymbol(new Uri("c://Users//mrc//Desktop//ikony//dzwonek3_32.png")) { Height = 20, Width = 20 };

            InterloperPicSymbol = new PictureMarkerSymbol(new Uri("c://Users//mrc//Desktop//ikony//atv_22.png")) { Height = 22, Width = 22 };

            DollarPicSymbol = new PictureMarkerSymbol(new Uri("c://Users//mrc//Desktop//ikony//dolar_64.png")) { Height = 32, Width = 32 };
        }
    }
}
