using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model
{
    public class SingleRoute
    {
        public virtual double Length { get; set; }
        public virtual LineString Geom { get; set; }
    }
}
