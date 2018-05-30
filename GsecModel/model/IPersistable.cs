using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model
{
    public interface IPersistable
    {
        void Create();
        void Update();
        void Delete();
    }
}
