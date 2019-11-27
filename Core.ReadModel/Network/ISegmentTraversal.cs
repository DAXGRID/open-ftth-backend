using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public interface ISegmentTraversal
    {
        INode StartRouteNode { get; set; }
        ISegment StartRouteSegment { get; set; }
        INode EndRouteNode { get; set; }
        ISegment EndRouteSegment { get; set; }
        List<INode> AllRouteNodes { get; set; }
        List<ISegment> AllRouteSegments { get; set; }
        List<ISegment> AllSegments { get; set; }
    }
}
