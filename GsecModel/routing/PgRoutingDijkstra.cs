using GeoAPI.Geometries;
using gsec.model;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql;
using System;
using System.Data;

namespace gsec.routing
{
    public static class PgRoutingDijkstra
    {
        /* Pani Olu - przepraszam! */
        static string queryFmt = @"
        WITH routing_result AS
        (
            /* merge pgr result and current road */
            SELECT ST_LineMerge(ST_Union(merged_pgr_route, splitted_current_road)) AS final_route 
            FROM
                /* 1st subquery - merge result of pgr_Dijkstra into single polyline */
                (
                    SELECT ST_LineMerge(ST_Union(e1.geom)) AS merged_pgr_route
                    FROM gsectopo.edge_data e1
                    WHERE e1.edge_id IN
                    (
                        SELECT id2 AS edge_id 
                        FROM pgr_Dijkstra('SELECT edge_id AS id, start_node AS source, end_node AS target, length AS cost FROM gsectopo.edge_data', {0}, {1}, false, false)
                    )
                ) AS pgrmerge,

        		/* 2nd subquery (actually 5th or 7th...) - split current road by ranger position */
		        (
                    SELECT(ST_Dump(ST_Split(ST_Snap(e2.geom, r.position, 0.001), r.position))).geom AS splitted_current_road 
                    FROM gsectopo.edge_data e2 JOIN ranger r ON ST_DWithin(e2.geom, r.position, 0.00001) 
                    WHERE e2.edge_id = {2} AND r.id = {3}
                ) AS roadsplit
        )
        SELECT final_route, ST_Length(final_route, false)
        FROM routing_result
        WHERE ST_GeometryType(final_route) = 'ST_LineString' /* pickup only polyline, not multiline */";

        public static SingleRoute Calculate(Ranger ranger, Road currentRoad, Crossing target)
        {
            // needs refactoring after testing
            string query1 = string.Format(queryFmt, currentRoad.Source.ID, target.ID, currentRoad.ID, ranger.ID);
            string query2 = string.Format(queryFmt, currentRoad.Target.ID, target.ID, currentRoad.ID, ranger.ID);

            DataTable dt1 = queryRoute(query1);
            DataTable dt2 = queryRoute(query2);
            
            double length1 = (double)dt1.Rows[0].ItemArray[1];
            double length2 = (double)dt2.Rows[0].ItemArray[1];
            Console.WriteLine("route lengths: {0} {1}", length1, length2);

            SingleRoute route = new SingleRoute();
            if (length1 < length2)
            {
                route.Length = length1;
                route.Geom = GeoTypeExtensions.FromEWKB((string)dt1.Rows[0].ItemArray[0]) as LineString;
            }
            else
            {
                route.Length = length2;
                route.Geom = GeoTypeExtensions.FromEWKB((string)dt1.Rows[1].ItemArray[0]) as LineString;
            }

            Console.WriteLine("picked up {0} route", route.Length);
            return route;
        }

        private static DataTable queryRoute(string query)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(query, Database.Connection);
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, Database.Connection);

                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];

                return dt;
            }
            catch (Exception e)
            {
                throw new GsecException("Cannot find route", e);
            }
        }
    }
}
