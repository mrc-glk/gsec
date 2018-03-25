using GeoAPI.Geometries;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Spatial.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model
{
    public class HSession
    {
        public static ISessionFactory Factory = null;

        static HSession()
        {
            Configuration c = new Configuration();
            c.Configure();
            c.AddAssembly(Assembly.GetCallingAssembly());
            Factory = c.BuildSessionFactory();
        }
    }
}
