using gsec.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.ui.events
{
    public class SensorSelectionEventArgs : EventArgs
    {
        public Sensor Sensor { get; set; }

        public SensorSelectionEventArgs(Sensor sensor)
        {
            Sensor = sensor;
        }
    }

    public delegate void SensorSelectionEventHandler(object sender, SensorSelectionEventArgs e);
}
