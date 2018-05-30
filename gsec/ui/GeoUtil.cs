using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.ui
{
    public static class GeoUtil
    {
        public static MapPoint GetNearestCoordinateInGraphicsCollection(MapPoint point, IList<Graphic> graphics)
        {
            ProximityResult nearest = null;
            MapPoint loc = point.ToWgs84();  // :)

            foreach (var graphic in graphics)
            {
                ProximityResult result = GeometryEngine.NearestCoordinate(graphic.Geometry, loc);
                if (nearest == null || result.Distance < nearest.Distance)
                {
                    nearest = result;
                }
            }

            return nearest?.Coordinate;
        }

        public static MapPoint GetRandomPointInGraphicsCollection(IList<Graphic> graphics)
        {
            Random random = new Random();
            Graphic graphic1 = graphics[random.Next(0, graphics.Count)];
            Geometry geom1 = graphic1.Geometry;

            if (geom1 is MapPoint)
                return geom1 as MapPoint;
            
            Envelope mbr = geom1.Extent;
            double x = random.NextDouble() * (mbr.XMax - mbr.XMin) + mbr.XMin;
            double y = random.NextDouble() * (mbr.YMax - mbr.YMin) + mbr.YMin;
            MapPoint randomLocation = new MapPoint(x, y, SpatialReferences.Wgs84);

            return GetNearestCoordinateInGraphicsCollection(randomLocation, new List<Graphic> { graphic1 });
        }

        public static Geometry GetBuffer(Geometry geometry, double meters)
        {
            Geometry buf = GeometryEngine.BufferGeodetic(geometry, meters, LinearUnits.Meters);
            return buf;
        }
    }
}
