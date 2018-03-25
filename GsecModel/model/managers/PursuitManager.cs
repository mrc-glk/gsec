using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model.managers
{
    public class PursuitManager : AbstractManager<Pursuit>
    {
        private static readonly PursuitManager instance = new PursuitManager();

        static PursuitManager() { }
        private PursuitManager() { }

        public override string GetTable() => "Pursuit";
        public static PursuitManager Instance { get => instance; }
    }
}
