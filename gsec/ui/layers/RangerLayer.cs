using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using gsec.model;
using gsec.ui.animations;
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
        Dictionary<Ranger, PursuitAnimation> pursuitAnimations = new Dictionary<Ranger, PursuitAnimation>();
        Dictionary<Ranger, FineAnimation> fineAnimations = new Dictionary<Ranger, FineAnimation>();

        public RangerLayer(List<Ranger> elements) : base(elements)
        {
        }

        public override void GenerateGraphics()
        {
            base.GenerateGraphics();

            foreach (Ranger ranger in Elements)
            {
                GenerateGraphicFor(ranger);
                // route? buffer?
            }
        }

        protected override void RemoveElementInternal(Ranger element)
        {
            BaseOverlay.Graphics.Remove(element.Graphic);
            BaseOverlay.Graphics.Remove(element.RangeGraphic);

            Elements.Remove(element);
            element.Delete();
        }

        protected override Ranger AddElementInternal(MapPoint position)
        {
            Ranger ranger = new Ranger();
            ranger.Position = position.ToNtsPoint();
            ranger.Name = "newranger";
            ranger.RState = RangerState.Free;
            ranger.Create();

            GenerateGraphicFor(ranger);
            Elements.Add(ranger);
            return ranger;
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
            element.Graphic = new Graphic(position, GeneralRenderers.RangerPicSymbol);
            BaseOverlay.Graphics.Add(element.Graphic);
        }

        public void ShowFineAnimation(Ranger ranger)
        {
            if (fineAnimations.ContainsKey(ranger) == false)
            {
                FineAnimation fine = new FineAnimation(ranger, FineHandled);
                fineAnimations.Add(ranger, fine);
                fine.Start();
            }
        }

        private void FineHandled(BaseAnimation anim)
        {
            var ranger = fineAnimations.FirstOrDefault(x => x.Value == anim).Key;
            fineAnimations.Remove(ranger);
        }
    }
}
