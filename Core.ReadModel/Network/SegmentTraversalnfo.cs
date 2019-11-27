using Core.ReadModel.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public class SegmentTraversalInfo : ISegmentTraversal
    {

        public INode StartRouteNode { get; set; }
        public ISegment StartRouteSegment { get; set; }
        public INode EndRouteNode { get; set; }
        public ISegment EndRouteSegment { get; set; }
        public List<INode> AllRouteNodes { get; set; }
        public List<ISegment> AllRouteSegments { get; set; }
        public List<ISegment> AllSegments { get; set; }
    }
}
