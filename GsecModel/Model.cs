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
        public List<SingleRoute> Routes;

        public static Dictionary<Type, IHManager> Managers = new Dictionary<Type, IHManager>
        {
            { typeof(Ranger), RangerManager.Instance },
            { typeof(Interloper), InterloperManager.Instance },
            { typeof(Sensor), SensorManager.Instance },
            { typeof(Pursuit), PursuitManager.Instance },
            { typeof(Road), RoadManager.Instance },
            { typeof(Crossing), CrossingManager.Instance },
            { typeof(SingleRoute), SingleRouteManager.Instance },
        };

        public Model()
        {
            Load();
            Database.Connect();
        }

        public void Load()
        {
            Routes = SingleRouteManager.Instance.List() as List<SingleRoute>;
            Rangers = RangerManager.Instance.List() as List<Ranger>;
            Roads = RoadManager.Instance.List() as List<Road>;
            Crossings = CrossingManager.Instance.List() as List<Crossing>;
            Pursuits = PursuitManager.Instance.List() as List<Pursuit>;
            Sensors = SensorManager.Instance.List() as List<Sensor>;
            Interlopers = InterloperManager.Instance.List() as List<Interloper>;
        }
    }
}
