using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Esri.ArcGISRuntime.Symbology;
using gsec.model;

namespace gsec.ui.animations
{
    public class AlarmAnimation : BaseAnimation
    {
        Sensor sensor;
        protected override double DurationSeconds => 3;

        public AlarmAnimation(Sensor sensor, Action<BaseAnimation> onFinish = null)
        {
            this.sensor = sensor;
            this.OnFinish = onFinish;
        }

        public override bool CanStart()
        {
            return sensor.AlarmGraphic.IsVisible == false;
        }

        public override bool CanStop()
        {
            return sensor.AlarmGraphic.IsVisible == true;
        }

        protected override void Update(double elapsedSeconds)
        {
            double maxRadius = Sensor.Range * 3;
            double pcRadious = Math.Abs(Math.Sin(1.75 * Math.PI * elapsedSeconds));
            double pcOpacity = elapsedSeconds / DurationSeconds;

            //SimpleFillSymbol s = sensor.AlarmGraphic.Symbol as SimpleFillSymbol;
            //var curColor = s.Color;
            //curColor.A = (byte) ((1.0 - Math.Sqrt(pcOpacity)) * 255);
            //Console.WriteLine("new color = {0}", curColor.A);
            //s.Color = curColor;
            sensor.AlarmGraphic.Geometry = GeoUtil.GetBuffer(sensor.Graphic.Geometry, pcRadious * maxRadius);
        }

        protected override void Init()
        {
            sensor.AlarmGraphic.IsVisible = true;
        }

        protected override void Finish()
        {
            sensor.AlarmGraphic.IsVisible = false;
        }
    }
}
