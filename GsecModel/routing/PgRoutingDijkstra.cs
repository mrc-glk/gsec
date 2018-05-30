using GeoAPI.Geometries;
using gsec.model;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql;
using System;
using System.Data;

namespace gsec.routing
{
    public class PgRoutingDijkstra : IRoutingAlgo
    {
        static string query = @"
        SELECT ST_LineMerge(ST_Union(e1.geom)) AS merged_pgr_route
        FROM gsectopo.edge_data e1
        WHERE e1.edge_id IN
        (
            SELECT id2 AS edge_id 
            FROM pgr_dijkstra('SELECT edge_id AS id, start_node AS source, end_node AS target, length AS cost FROM gsectopo.edge_data', {0}, {1}, false, false)
        )";

        public SingleRoute GetRouteFromCrossing(MobileUnit obj, Crossing source, Crossing target)
        {
            string subquery = string.Format(query, source.ID, target.ID);
            return PgRoutingCommons.GetRouteFromCrossing(subquery, obj, source, target);
        }
        
        public SingleRoute GetRouteFromMidRoad(MobileUnit obj, Road currentRoad, Crossing target)
        {
            string subquery1 = string.Format(query, currentRoad.Source.ID, target.ID);
            string subquery2 = string.Format(query, currentRoad.Target.ID, target.ID);
            return PgRoutingCommons.GetRouteFromMidRoad(subquery1, subquery2, obj, currentRoad, target);
        }
    }
}
