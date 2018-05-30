using Esri.ArcGISRuntime.Symbology;
using gsec.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.ui.animations
{
    public class NewRouteAnimation : BaseAnimation
    {
        private readonly SingleRoute route;

        protected override double DurationSeconds => 1;
        
        public NewRouteAnimation(SingleRoute route, Action<BaseAnimation> onFinish)
        {
            this.route = route;
            this.OnFinish = onFinish;

            if (route.Graphic.Symbol == null)
            {
                route.Graphic.Symbol = GeneralRenderers.AltRouteSymbol.Clone();
                ViewModel.Instance.RouteOverlay.Graphics.Add(route.Graphic);
            }
        }

        protected override void Update(double elapsedSeconds)
        {
            double perc = elapsedSeconds / DurationSeconds;
            double opac = Math.Min(1.0f, Math.Cos(perc * Math.PI / 2));
            opac = Math.Max(0.0f, opac);
            
            SimpleLineSymbol symbol = route.Graphic.Symbol as SimpleLineSymbol;
            var curColor = symbol.Color;
            curColor.A = (byte) (opac * 255);
            symbol.Color = curColor;
        }

        protected override void Finish()
        {
            ViewModel.Instance.RouteOverlay.Graphics.Remove(route.Graphic);
            base.Finish();
        }
    }
}
