using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using gsec.model;
using gsec.model.managers;
using gsec.ui.events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.ui.layers
{
    public class RangerLayer : AbstractLayer<Ranger>
    {        
        public RangerLayer(List<Ranger> elements) : base(elements)
        {
        }

        public override void GenerateGraphics()
        {
            base.GenerateGraphics();

            foreach (Ranger ranger in Elements)
            {
                GenerateGraphicFor(ranger);
                BaseOverlay.Graphics.Add(ranger.Graphic);
                // route? buffer?
            }
        }

        public override void RemoveElement(Ranger element)
        {
            BaseOverlay.Graphics.Remove(element.Graphic);
            BaseOverlay.Graphics.Remove(element.RangeGraphic);

            RangerManager.Instance.Delete(element);
        }

        public override void AddElement(MapPoint position)
        {
            Ranger ranger = new Ranger();
            ranger.Position = position.ToNtsPoint();
            ranger.Name = "newranger";
            ranger.State = RangerState.Free;

            long id = RangerManager.Instance.Create(ranger);
            if (id != -1)
            {
                GenerateGraphicFor(ranger);
                BaseOverlay.Graphics.Add(ranger.Graphic);
                Elements.Add(ranger);
            }
        }

        public override void Select(Ranger element)
        {
            // send popup event?
            // show buffer?
            base.Select(element);
        }

        public override void Unselect(Ranger element)
        {
            base.Unselect(element);
        }

        protected override void GenerateGraphicFor(Ranger element)
        {
            MapPoint position = element.Position.ToEsriPoint();
            Graphic graphic = new Graphic(position, GeneralRenderers.RangerSymbol);
            element.Graphic = graphic;

            // range graphic is created on-demand only
        }
    }
}
