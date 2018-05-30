using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model.managers
{
    public class InterloperManager : AbstractManager<Interloper>
    {
        private static readonly InterloperManager instance = new InterloperManager();

        static InterloperManager() { }
        private InterloperManager()
        {
        }

        public static InterloperManager Instance { get => instance; }
    }
}
