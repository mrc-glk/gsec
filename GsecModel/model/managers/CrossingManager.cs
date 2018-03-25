using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model.managers
{
    public sealed class CrossingManager : AbstractManager<Crossing>
    {
        private static readonly CrossingManager instance = new CrossingManager();

        static CrossingManager() { }
        private CrossingManager() { }

        public override string GetTable() => "Crossing";
        public static CrossingManager Instance { get => instance; }
    }
}
