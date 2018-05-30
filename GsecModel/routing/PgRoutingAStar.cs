using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gsec.model;

namespace gsec.routing
{
    public class PgRoutingAStar : IRoutingAlgo
    {
        enum HEURISTIC {
            //  TODO
        }

        static string query = @"
        SELECT ST_LineMerge(ST_Union(e1.geom)) AS merged_pgr_route
        FROM gsectopo.edge_data e1
        WHERE e1.edge_id IN
        (
            SELECT edge AS edge_id 
            FROM pgr_astar
            (
                'SELECT edges.edge_id AS id, edges.start_node AS source, edges.end_node AS target, edges.length AS cost,
                        ST_X(startnodes.geom) AS x1,
                        ST_Y(startnodes.geom) AS y1,
                        ST_X(endnodes.geom) AS x2,
                        ST_Y(endnodes.geom) AS y2
                FROM gsectopo.edge_data edges
                    JOIN gsectopo.node startnodes ON (edges.start_node = startnodes.node_id)
                    JOIN gsectopo.node endnodes ON (edges.end_node = endnodes.node_id)
                ', 
                {0}, {1}, directed:=false, heuristic:=3
            )
        )";
/*
 *
                        CAST(0.0 as FLOAT8) AS x1,
                        CAST(0.0 as FLOAT8) AS y1,
                        CAST(0.0 as FLOAT8) AS x2,
                        CAST(0.0 as FLOAT8) AS y2
*/
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
