using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using gsec.model;
using gsec.model.managers;
using gsec.ui.animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace gsec.ui.layers
{
    public class SensorLayer : AbstractLayer<Sensor>
    {
        Dictionary<Sensor, AlarmAnimation> alarmAnimations = new Dictionary<Sensor, AlarmAnimation>();

        public SensorLayer(List<Sensor> elements) : base(elements)
        {
        }

        public override void GenerateGraphics()
        {
            base.GenerateGraphics();

            foreach (Sensor sensor in Elements)
            {
                GenerateGraphicFor(sensor);
            }
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
            element.Graphic = new Graphic(position, GeneralRenderers.SensorPicSymbol);
            
            Polygon range = GeometryEngine.BufferGeodetic(position, Sensor.Range, LinearUnits.Meters) as Polygon;
            element.RangeGraphic = new Graphic(range, GeneralRenderers.SensorRangeFillSymbol);
            element.RangeGraphic.IsVisible = true;

            Polygon alarm = GeometryEngine.BufferGeodetic(position, 1.0, LinearUnits.Meters) as Polygon;
            element.AlarmGraphic = new Graphic(range, GeneralRenderers.SensorAlarmFillSymbol);
            element.AlarmGraphic.IsVisible = false;

            BaseOverlay.Graphics.Add(element.RangeGraphic);
            BaseOverlay.Graphics.Add(element.AlarmGraphic);
            BaseOverlay.Graphics.Add(element.Graphic);
        }

        protected override Sensor AddElementInternal(MapPoint position)
        {
            Sensor sensor = new Sensor();
            sensor.Position = position.ToNtsPoint();

            sensor.Create();

            GenerateGraphicFor(sensor);
            Elements.Add(sensor);
            return sensor;
        }

        protected override void RemoveElementInternal(Sensor element)
        {
            BaseOverlay.Graphics.Remove(element.Graphic);
            BaseOverlay.Graphics.Remove(element.RangeGraphic);
            BaseOverlay.Graphics.Remove(element.AlarmGraphic);

            Elements.Remove(element);
            element.Delete();
        }

        public void UpdateRanges()
        {
            foreach (Sensor sensor in Elements)
            {
                sensor.RangeGraphic.Geometry = GeometryEngine.BufferGeodetic(sensor.Position.ToEsriPoint(), Sensor.Range, LinearUnits.Meters) as Polygon;
            }
        }

        public void SetRangeVisibility(bool visibility)
        {
            foreach (Sensor sensor in Elements)
            {
                sensor.RangeGraphic.IsVisible = visibility;
            }
        }

        public void RaiseAlarm(Sensor sensor)
        {
            if (alarmAnimations.ContainsKey(sensor) == false)
            {
                AlarmAnimation alarm = new AlarmAnimation(sensor, AlarmHandled);
                alarmAnimations.Add(sensor, alarm);
                alarm.Start();
            }
        }

        public void AlarmHandled(BaseAnimation anim)
        {
            var sensor = alarmAnimations.FirstOrDefault(x => x.Value == anim).Key;
            alarmAnimations.Remove(sensor);
        }

        public void CancelAlarms()
        {
            // should use queue instead
            AlarmAnimation[] workCopy = alarmAnimations.Values.ToArray();

            foreach (var alarm in workCopy)
            {
                alarm.Stop();
            }
        }
    }
}
