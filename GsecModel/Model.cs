using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using gsec.model;
using gsec.model.managers;

namespace gsec
{
    public class Model
    {
        public List<Ranger> Rangers;
        public List<Interloper> Interlopers;
        public List<Sensor> Sensors;
        public List<Pursuit> Pursuits;
        public List<Road> Roads;
        public List<Crossing> Crossings;

        public Model()
        {
            Load();
            Database.Connect();
        }

        public void Load()
        {
            Rangers = RangerManager.Instance.List() as List<Ranger>;
            Roads = RoadManager.Instance.List() as List<Road>;
            Crossings = CrossingManager.Instance.List() as List<Crossing>;
            Pursuits = PursuitManager.Instance.List() as List<Pursuit>;
            Sensors = SensorManager.Instance.List() as List<Sensor>;
            Interlopers = InterloperManager.Instance.List() as List<Interloper>;
        }
    }
}
