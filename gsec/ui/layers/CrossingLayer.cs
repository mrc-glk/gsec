using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using gsec.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.ui.layers
{
    public class CrossingLayer : AbstractLayer<Crossing>
    {
        public CrossingLayer(List<Crossing> elements) : base(elements)
        {
        }

        public override void GenerateGraphics()
        {
            base.GenerateGraphics();
            
            foreach (Crossing crossing in Elements)
            {
                GenerateGraphicFor(crossing);
                BaseOverlay.Graphics.Add(crossing.Graphic);
            }
        }

        public override void RemoveElement(Crossing element)
        {
            throw new NotImplementedException();
        }

        public override void Select(Crossing element)
        {
            element.Graphic.IsSelected = true;
        }

        public override void Unselect(Crossing element)
        {
            element.Graphic.IsSelected = false;
        }

        protected override void GenerateGraphicFor(Crossing element)
        {
            MapPoint position = element.Position.ToEsriPoint();
            Graphic graphic = new Graphic(position, GeneralRenderers.CrossingSymbol);
            element.Graphic = graphic;

        }
    }
}
