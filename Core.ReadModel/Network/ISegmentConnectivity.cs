using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public interface ISegmentConnectivity
    {
        INode StartRouteNode { get; set; }
        ILineSegment StartRouteSegment { get; set; }
        INode EndRouteNode { get; set; }
        ILineSegment EndRouteSegment { get; set; }
        List<INode> AllRouteNodes { get; set; }
        List<ILineSegment> AllRouteSegments { get; set; }
        List<ILineSegment> AllSegments { get; set; }
    }
}
