using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model
{
    public enum RangerState
    {
        Free,
        InPursuit,
    }

    public class RangerStateType : EnumStringType<RangerState>
    {
    }
}
