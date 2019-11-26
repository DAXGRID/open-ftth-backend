using Core.GraphSupport.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public interface ILineSegment : INetworkElement
    {
        LineSegmentRelationTypeEnum RelationType(Guid pointOfInterestId);
        ILine Line { get; set; }
        List<ILineSegment> Parents { get; set; }
        List<ILineSegment> Children { get; set; }
        Guid FromRouteNodeId { get; set; }
        Guid ToRouteNodeId { get; set; }
        INode FromRouteNode { get; set; }
        INode ToRouteNode { get; set; }
        Guid FromNodeId { get; set; }
        Guid ToNodeId { get; set; }
        INode FromNode { get; set; }
        INode ToNode { get; set; }
        int SequenceNumber{ get; set; }
    }
}
