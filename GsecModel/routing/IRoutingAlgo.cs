using gsec.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.routing
{
    public interface IRoutingAlgo
    {
        SingleRoute GetRouteFromMidRoad(MobileUnit obj, Road currentRoad, Crossing target);
        SingleRoute GetRouteFromCrossing(MobileUnit obj, Crossing source, Crossing target);
    }
}
