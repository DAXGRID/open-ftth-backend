using ConduitNetwork.Events.Model;
using Core.ReadModel.Network;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.ReadModel
{
    public class ConduitConnectivityInfo : ISegmentTraversal
    {

        public INode StartRouteNode { get; set; }
        public ISegment StartRouteSegment { get; set; }
        public INode EndRouteNode { get; set; }
        public ISegment EndRouteSegment { get; set; }
        public List<INode> AllRouteNodes { get; set; }
        public List<ISegment> AllRouteSegments { get; set; }
        public List<ISegment> AllSegments { get; set; }

        /*
        public RouteNodeInfo StartRouteNode { get; set; }
        public RouteSegmentInfo StartRouteSegment { get; set; }
        public RouteNodeInfo EndRouteNode { get; set; }
        public RouteSegmentInfo EndRouteSegment { get; set; }
        public List<RouteNodeInfo> AllRouteNodes { get; set; }
        public List<RouteSegmentInfo> AllRouteSegments { get; set; }
        public List<ConduitSegmentInfo> AllConduitSegments { get; set; }
        */
    }
}
