using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Events
{
   public  class PlannedRouteSegmentSplitByNode
    {
        public Guid Id { get; set; }
        public Guid SplitNodeId { get; set; }
    }
}
