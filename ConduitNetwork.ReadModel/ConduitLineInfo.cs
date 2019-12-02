using ConduitNetwork.Events.Model;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.ReadModel
{
    public class ConduitLineInfo
    {
        public RouteNodeInfo StartRouteNode { get; set; }
        public RouteSegmentInfo StartRouteSegment { get; set; }
        public RouteNodeInfo EndRouteNode { get; set; }
        public RouteSegmentInfo EndRouteSegment { get; set; }
        public List<RouteNodeInfo> AllRouteNodes { get; set; }
        public List<RouteSegmentInfo> AllRouteSegments { get; set; }
        public List<ConduitSegmentInfo> AllSegments { get; set; }
    }
}