using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Events
{
   public  class RouteSegmentAdded
    {
        public Guid RouteSegmentId { get; set; }
        public Guid FromNodeId { get; set; }
        public Guid ToNodeId { get; set; }
        public RouteSegmentKindEnum SegmentKind { get; set; }
        public Geometry InitialGeometry { get; set; }
    }
}
