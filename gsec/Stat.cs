using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec
{
    public class Stat
    {
        public int NrSensors { get; set; }
        public int NrAlarms { get; set; }
        public int NrRangers { get; set; }
        public int NrCaught { get; set; }
        public int NrEscaped { get; set; }
        public int NrInterlopers { get; set; }
    }
}
