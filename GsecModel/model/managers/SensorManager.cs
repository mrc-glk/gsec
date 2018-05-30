using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model.managers
{
    public class SensorManager : AbstractManager<Sensor>
    {
        private static readonly SensorManager instance = new SensorManager();

        static SensorManager() { }
        private SensorManager()
        {
        }

        public static SensorManager Instance { get => instance; }
    }
}
