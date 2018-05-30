using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using gsec.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace gsec.ui.animations
{
    public class FineAnimation : BaseAnimation
    {
        private int meters { get { return Ranger.FineRange; } }
        Ranger ranger;
        Graphic fineGraphic = new Graphic();
        byte defaultAlpha = 192;

        protected override double DurationSeconds => 1;

        public FineAnimation(Ranger ranger, Action<BaseAnimation> onFinish = null)
        {
            this.ranger = ranger;
            OnFinish = onFinish;
        }

        protected override void Update(double elapsedSeconds)
        {
            double perc = elapsedSeconds / DurationSeconds;

#if true
            fineGraphic.Geometry = GeoUtil.GetBuffer(ranger.Graphic.Geometry, meters);

            SimpleFillSymbol symbol = fineGraphic.Symbol as SimpleFillSymbol;
            var curColor = symbol.Color;
            curColor.A = (byte)((1.0 - Math.Sqrt(perc)) * defaultAlpha);
symbol.Color = curColor;
#else
            var sym = fineGraphic.Symbol as PictureMarkerSymbol;
            double nv = 1.0 - Math.Sqrt(perc);
            nv = Math.Min(1.0, nv);
            nv = Math.Max(0.0, nv);
            sym.Opacity = nv;
#endif
        }

        protected override void Init()
        {
            // this one should be an instersection of ranger buffer and current road buffer
            fineGraphic.Geometry = GeoUtil.GetBuffer(ranger.Graphic.Geometry, meters);
#if true
            fineGraphic.Symbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(defaultAlpha, 0, 0, 0), null);
#else
            fineGraphic.Symbol = GeneralRenderers.DollarPicSymbol.Clone();
#endif
            ranger.Graphic.GraphicsOverlay.Graphics.Add(fineGraphic);
        }

        protected override void Finish()
        {
            ranger.Graphic.GraphicsOverlay.Graphics.Remove(fineGraphic);
        }
    }
}
