using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model.managers
{
    public class SingleRouteManager : AbstractManager<SingleRoute>
    {
        private static readonly SingleRouteManager instance = new SingleRouteManager();

        static SingleRouteManager() { }
        private SingleRouteManager()
        {
        }

        public static SingleRouteManager Instance { get => instance; }
    }
}
