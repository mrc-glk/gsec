using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using gsec.model;

namespace gsec.ui.layers
{
    public class RoadLayer : AbstractLayer<Road>
    {
        public RoadLayer(List<Road> elements) : base(elements)
        {
        }

        public override void GenerateGraphics()
        {
            base.GenerateGraphics();

            foreach (Road road in Elements)
            {
                GenerateGraphicFor(road);
            }
        }

        public override void Select(Road element)
        {
            element.Graphic.IsSelected = true;
        }

        public override void Unselect(Road element)
        {
            element.Graphic.IsSelected = false;
        }

        protected override void GenerateGraphicFor(Road element)
        {
            Polyline polyline = element.Geom.ToEsriPolyline();
            element.Graphic = new Graphic(polyline, GeneralRenderers.RoadSymbol);
            BaseOverlay.Graphics.Add(element.Graphic);
        }
    }
}
