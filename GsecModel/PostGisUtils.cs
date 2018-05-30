using Esri.ArcGISRuntime.Geometry;
using gsec.model;
using gsec.model.managers;
using NetTopologySuite.Geometries;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec
{
    public static class PostGisUtils
    {
        static string queryCutRouteAtPoint = @"
        WITH interpolated AS 
        (
            SELECT ST_LineInterpolatePoint(geom, {0}) AS pt FROM singleroute
        )
        SELECT (ST_Dump(ST_Split(ST_Snap(r.geom, i.pt, 0.001), i.pt))).geom AS splitted_route 
        FROM singleroute r, interpolated i
        WHERE r.id = {1};";

        public static List<Polyline> CutRouteAtPoint(SingleRoute route, double prc)
        {
            List<Polyline> polylines = new List<Polyline>();

            string query = string.Format(queryCutRouteAtPoint, prc, route.ID);

            DataTable dt = PostGisUtils.Query(query);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                LineString line = GeoTypeExtensions.FromEWKB((string)(dt.Rows[i].ItemArray[0])) as LineString;
                polylines.Add(line.ToEsriPolyline());
            }

            return polylines;
        }

        static object lkobj = new object();
        public static DataTable Query(string query)
        {
            lock (lkobj)
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
                    throw new GsecException("failed to DB op", e);
                }
            }
        }

        private static string internalGetEWKB(string table, string geomColumn, string idColumn, long id)
        {
            string query = string.Format("SELECT {0} FROM {1} WHERE {2} = {3};", geomColumn, table, idColumn, id);
            DataTable dt = Query(query);
            if (dt.Rows.Count != 0)
            {
                return dt.Rows[0].ItemArray[0] as string;
            }
            return null;
        }

        public static string GetEWKB(IDisplayableGeoElement x)
        {
            IHManager manager = Model.Managers[x.GetType()];
            return internalGetEWKB(manager.GetTable(), manager.GetGeometryColumn(), manager.GetIdColumn(), x.ID);
        }
    }
}
