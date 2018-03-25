using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model
{
    public class Pursuit
    {
        public virtual long ID { get; set; }
        public virtual Ranger Ranger { get; set; }
        public virtual Interloper Interloper { get; set; }
    }
}
