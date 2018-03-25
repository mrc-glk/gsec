using Esri.ArcGISRuntime.UI;
using gsec.model;
using gsec.ui.layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.ui
{
    public static class GsecLayerExtensions
    {
        public static Graphic GetRandomGraphic<T>(this AbstractLayer<T> layer) where T : IDisplayableGeoElement
        {
            Random random = new Random();
            GraphicCollection graphics = layer.GetOverlay().Graphics;
            if (graphics.Count == 0)
                return null;

            return graphics[random.Next(0, graphics.Count)];
        }
    }
}
