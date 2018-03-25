using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Esri.ArcGISRuntime.Geometry;
using NpgsqlTypes;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;
using NetTopologySuite.IO;

namespace gsec
{
    public static class GeoTypeExtensions
    {
        public static Point ToNtsPoint(this MapPoint pt)
        {
            Coordinate coord = new Coordinate(pt.X, pt.Y);
            return new Point(coord);
        }

        public static MapPoint ToEsriPoint(this Point pt)
        {
            return new MapPoint(pt.X, pt.Y, SpatialReferences.Wgs84);
        }

        public static MapPoint ToWgs84(this MapPoint pt)
        {
            return GeometryEngine.Project(pt, SpatialReferences.Wgs84) as MapPoint;           
        }

        public static LineString ToNtsLine(this Polyline polyline)
        {
            // TODO HACK Parts[0]!!!
            var points = polyline.Parts[0].Points;
            List<Coordinate> coords = new List<Coordinate>();

            foreach (var point in points)
            {
                coords.Add(new Coordinate(point.X, point.Y));
            }

            LineString ntsLine = new LineString(coords.ToArray());
            return ntsLine;
        }

        public static Polyline ToEsriPolyline(this LineString line)
        {
            PolylineBuilder bld = new PolylineBuilder(SpatialReferences.Wgs84);

            foreach (var coord in line.Coordinates)
            {
                bld.AddPoint(new MapPoint(coord.X, coord.Y, SpatialReferences.Wgs84));
            }

            return bld.ToGeometry();
        }

        public static GeoAPI.Geometries.IGeometry FromEWKB(string ewkb)
        {
            byte[] bytes = CainKellyeStringToByteArrayFastest(ewkb);
            return new WKBReader().Read(bytes) as LineString;
        }
        
        public static byte[] CainKellyeStringToByteArrayFastest(string hex)
        {
            // thank you https://stackoverflow.com/users/356577/cainkellye!
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}
