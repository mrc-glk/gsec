using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model.managers
{
    public class RangerManager : AbstractManager<Ranger>
    {
        private static readonly RangerManager instance = new RangerManager();

        static RangerManager() { }
        private RangerManager() { }

        public override string GetTable() => "Ranger";
        public static RangerManager Instance { get => instance; }
    }
}
