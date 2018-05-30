using gsec.model;
using NetTopologySuite.Geometries;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.routing
{
    public static class PgRoutingCommons
    {
        /* Pani Olu - przepraszam! */
        static string queryRoadToCrossing = @"
        WITH routing_result AS
        (
            /* merge pgr result and current road */
            SELECT ST_LineMerge(ST_Union(merged_pgr_route, splitted_current_road)) AS final_route 
            FROM
                /* 1st subquery - merge result of pgr_Dijkstra into single polyline */
                (
                    {0}
                ) AS pgrmerge,

        		/* 2nd subquery (actually 5th or 7th...) - split current road by object position */
		        (
                    SELECT(ST_Dump(ST_Split(ST_Snap(e2.geom, r.geom, 0.001), r.geom))).geom AS splitted_current_road 
                    FROM gsectopo.edge_data e2 JOIN {1} r ON ST_DWithin(e2.geom, r.geom, 0.00001) 
                    WHERE e2.edge_id = {2} AND r.id = {3}
                ) AS roadsplit
        )
        SELECT final_route, ST_Length(final_route, false)
        FROM routing_result
        WHERE ST_GeometryType(final_route) = 'ST_LineString' /* pickup only polyline, not multiline */";

        public static SingleRoute GetRouteFromMidRoad(string subquery1, string subquery2, MobileUnit obj, Road currentRoad, Crossing target)
        {
            // needs refactoring after testing
            string table = Model.Managers[obj.GetType()].GetTable();

            string query1 = string.Format(queryRoadToCrossing, subquery1, table, currentRoad.ID, obj.ID);
            string query2 = string.Format(queryRoadToCrossing, subquery2, table, currentRoad.ID, obj.ID);

            DataTable dt1 = PostGisUtils.Query(query1);
            DataTable dt2 = PostGisUtils.Query(query2);

            SingleRoute route = new SingleRoute();
            double length1 = double.MaxValue;
            double length2 = double.MaxValue;

            if (dt1.Rows.Count == 1)
                length1 = (double)dt1.Rows[0].ItemArray[1];
            if (dt2.Rows.Count == 1)
                length2 = (double)dt2.Rows[0].ItemArray[1];

            
            Console.WriteLine("route lengths: {0} {1}", length1, length2);

            if (length1 < length2)
            {
                route.Length = length1;
                string ewkb = ReverseIfNeeded((string)dt1.Rows[0].ItemArray[0], PostGisUtils.GetEWKB(obj));
                route.Geom = GeoTypeExtensions.FromEWKB(ewkb) as LineString;
            }
            else if (length1 > length2)
            {
                route.Length = length2;
                string ewkb = ReverseIfNeeded((string)dt2.Rows[0].ItemArray[0], PostGisUtils.GetEWKB(obj));
                route.Geom = GeoTypeExtensions.FromEWKB(ewkb) as LineString;
            }
            else
            {
                //throw new GsecException("can't find any route");
                Console.WriteLine("NO ROUTE SHOULD THROW EXCEPTION");
                return null;
            }

            return route;
        }

        static string queryCrossingToCrossing = @"
        SELECT merged_pgr_route, ST_Length(merged_pgr_route, false)
        FROM
        (
            {0}
        ) as pgrmerge";

        public static SingleRoute GetRouteFromCrossing(string subquery, MobileUnit obj, Crossing source, Crossing target)
        {
            string query = string.Format(queryCrossingToCrossing, subquery);
            DataTable dt = PostGisUtils.Query(query);

            if (dt.Rows.Count == 0 || dt.Rows[0].ItemArray[0] is DBNull)
            {
                //throw new GsecException("can't find any route");
                Console.WriteLine("NO ROUTE SHOULD THROW EXCEPTION");
                return null;
            }

            string ewkb = (string)dt.Rows[0].ItemArray[0];
            ewkb = ReverseIfNeeded(ewkb, PostGisUtils.GetEWKB(source));

            SingleRoute route = new SingleRoute();
            route.Geom = GeoTypeExtensions.FromEWKB(ewkb) as LineString;
            route.Length = (double)dt.Rows[0].ItemArray[1];

            return route;
        }
        
        public static string ReverseIfNeeded(string ewkbRoute, string ewkbPosition)
        {
            string queryReverseRoute = string.Format(@"
            SELECT CASE 
                   WHEN ST_DWithin(ST_EndPoint('{0}'), '{1}', 0.0001) = TRUE THEN ST_Reverse('{0}') 
                   ELSE '{0}'
                   END AS geom", ewkbRoute, ewkbPosition);

            DataTable dt = PostGisUtils.Query(queryReverseRoute);
            if (dt.Rows.Count == 0)
                throw new GsecException("failed to reverse route");

            string ewkbFixed = (string)dt.Rows[0].ItemArray[0];
            return ewkbFixed;
        }

    }
}
