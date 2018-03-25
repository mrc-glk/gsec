using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model.managers
{
    public class RoadManager : AbstractManager<Road>
    {
        private static readonly RoadManager instance = new RoadManager();

        static RoadManager() { }
        private RoadManager() { }

        public override string GetTable() => "Road";
        public static RoadManager Instance { get => instance; }
    }
}
