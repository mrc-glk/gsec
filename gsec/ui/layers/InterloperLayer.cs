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
        public bool ShowOnlyWithinSensorRange { get; set; } = false;

        public InterloperLayer(List<Interloper> elements) : base(elements)
        {
        }

        public override void GenerateGraphics()
        {
            base.GenerateGraphics();

            foreach (Interloper interloper in Elements)
            {
                GenerateGraphicFor(interloper);
            }
        }

        protected override Interloper AddElementInternal(MapPoint position)
        {
            Interloper interloper = new Interloper();
            interloper.Position = position.ToNtsPoint();
            interloper.Create();

            GenerateGraphicFor(interloper);
            Elements.Add(interloper);
            
            return interloper;
        }

        protected override void RemoveElementInternal(Interloper element)
        {
            BaseOverlay.Graphics.Remove(element.Graphic);
            Elements.Remove(element);
            element.Delete();
        }

        protected override void GenerateGraphicFor(Interloper element)
        {
            MapPoint position = element.Position.ToEsriPoint();
            element.Graphic = new Graphic(position, GeneralRenderers.InterloperPicSymbol);
            BaseOverlay.Graphics.Add(element.Graphic);

            if (ShowOnlyWithinSensorRange != false)
                element.Graphic.IsVisible = false;
        }
    }
}
