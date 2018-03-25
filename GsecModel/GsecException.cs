using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec
{
    public class GsecException : Exception
    {
        public GsecException(string msg) : base(msg) { }
        public GsecException(Exception ex) : base(ex.Message, ex) { }
        public GsecException(string msg, Exception ex) : base(msg, ex) { }
    }
}
