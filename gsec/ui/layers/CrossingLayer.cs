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
        public List<Crossing> Leafs = new List<Crossing>();

        public CrossingLayer(List<Crossing> elements) : base(elements)
        {
        }

        public override void GenerateGraphics()
        {
            base.GenerateGraphics();
            
            foreach (Crossing crossing in Elements)
            {
                GenerateGraphicFor(crossing);
            }
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
            element.Graphic = new Graphic(position, GeneralRenderers.CrossingSymbol);
            Console.WriteLine("generated graphi for crossing {0}", element.ID);
            BaseOverlay.Graphics.Add(element.Graphic);
        }
    }
}
