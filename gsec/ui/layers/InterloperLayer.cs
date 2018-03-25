using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using gsec.model;
using gsec.model.managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.ui.layers
{
    public class InterloperLayer : AbstractLayer<Interloper>
    {
        public InterloperLayer(List<Interloper> elements) : base(elements)
        {
        }

        public override void GenerateGraphics()
        {
            base.GenerateGraphics();

            foreach (Interloper interloper in Elements)
            {
                GenerateGraphicFor(interloper);
                BaseOverlay.Graphics.Add(interloper.Graphic);
            }
        }

        public override void AddElement(MapPoint position)
        {
            Interloper interloper = new Interloper();
            interloper.Position = position.ToNtsPoint();
            
            long id = InterloperManager.Instance.Create(interloper);
            if (id != -1)
            {
                GenerateGraphicFor(interloper);
                BaseOverlay.Graphics.Add(interloper.Graphic);
                Elements.Add(interloper);
            }
        }

        public override void RemoveElement(Interloper element)
        {
            BaseOverlay.Graphics.Remove(element.Graphic);
            InterloperManager.Instance.Delete(element);
        }

        protected override void GenerateGraphicFor(Interloper element)
        {
            MapPoint position = element.Position.ToEsriPoint();
            Graphic graphic = new Graphic(position, GeneralRenderers.InterloperSymbol);
            element.Graphic = graphic;
        }
    }
}
