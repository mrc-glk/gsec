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
    public class SensorLayer : AbstractLayer<Sensor>
    {
        public SensorLayer(List<Sensor> elements) : base(elements)
        {
        }

        public override void GenerateGraphics()
        {
            base.GenerateGraphics();

            foreach (Sensor sensor in Elements)
            {
                GenerateGraphicFor(sensor);
                BaseOverlay.Graphics.Add(sensor.Graphic);
            }
        }

        public override void RemoveElement(Sensor element)
        {
            BaseOverlay.Graphics.Remove(element.Graphic);
            BaseOverlay.Graphics.Remove(element.RangeGraphic);

            SensorManager.Instance.Delete(element);
        }

        public override void Select(Sensor element)
        {
            element.RangeGraphic.IsVisible = true;
            BaseOverlay.Graphics.Add(element.RangeGraphic);
        }

        public override void Unselect(Sensor element)
        {
            element.RangeGraphic.IsVisible = false;
            BaseOverlay.Graphics.Remove(element.RangeGraphic);
        }

        protected override void GenerateGraphicFor(Sensor element)
        {
            MapPoint position = element.Position.ToEsriPoint();
            Graphic graphic = new Graphic(position, GeneralRenderers.SensorSymbol);
            element.Graphic = graphic;

            // sensor is not movable object so we can fix-up the position of ranging buffer
            Polygon range = GeometryEngine.BufferGeodetic(position, Sensor.Range, LinearUnits.Meters) as Polygon;
            Graphic rangeGraphic = new Graphic(range, GeneralRenderers.SensorRangeSymbol);
            element.RangeGraphic = rangeGraphic;
            rangeGraphic.IsVisible = false;
        }

        public override void AddElement(MapPoint position)
        {
            Sensor sensor = new Sensor();
            sensor.Position = position.ToNtsPoint();

            long id = SensorManager.Instance.Create(sensor);
            if (id != -1)
            {
                GenerateGraphicFor(sensor);
                BaseOverlay.Graphics.Add(sensor.Graphic);
                Elements.Add(sensor);
            }
        }
    }
}
