using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model
{
    public static class Topology
    {
        // pgr_nodeNetwork broken
        //   https://github.com/pgRouting/pgrouting/issues/280
        //   https://github.com/pgRouting/pgrouting/pull/282
        // 
        // using PostGIS topology as suggested by yobiSource
        //   https://github.com/pgRouting/pgrouting/issues/419
        //   

        static string topoName = "gsectopo";

        static string qDropTopo = "SELECT DropTopology('gsectopo');";
        static string qCreateTopo = "SELECT CreateTopology('gsectopo', 4326);";
        static string qAddTopoGeomColumn = "SELECT AddTopoGeometryColumn('gsectopo', 'public', 'road', 'topogeom', 'LINE');";
        static string qUpdateTopoGeom = "UPDATE road SET topogeom = ToTopoGeom(geom, 'gsectopo', 1);";
        static string qAddLengthColumn = "ALTER TABLE gsectopo.edge_data ADD COLUMN length double precision;";
        static string qCalcLengthColumn = "UPDATE gsectopo.edge_data SET length = ST_Length(geom, false);";

        public static bool Rebuild()
        {
            int ret = -1;

            // very rough :) but i'll fix it later... (yeah sure)

            ret = Database.ExecuteSqlCommand(qDropTopo);
            Console.WriteLine("qDropTopo result = {0}", ret);
            if (ret <= 0)
                return false;

            ret = Database.ExecuteSqlCommand(qCreateTopo);
            Console.WriteLine("qCreateTopo result = {0}", ret);
            if (ret <= 0)
                return false;

            ret = Database.ExecuteSqlCommand(qAddTopoGeomColumn);
            Console.WriteLine("qAddTopoGeomColumn result = {0}", ret);
            if (ret <= 0)
                return false;

            ret = Database.ExecuteSqlCommand(qUpdateTopoGeom);
            Console.WriteLine("qUpdateTopoGeom result = {0}", ret);
            if (ret <= 0)
                return false;

            ret = Database.ExecuteSqlCommand(qAddLengthColumn);
            Console.WriteLine("qAddLengthColumn result = {0}", ret);
            if (ret <= 0)
                return false;

            ret = Database.ExecuteSqlCommand(qCalcLengthColumn);
            Console.WriteLine("qCalcLengthColumn result = {0}", ret);
            if (ret <= 0)
                return false;

            Console.WriteLine("Topology successfully rebuilt");
            return true;
        }
    }
}
