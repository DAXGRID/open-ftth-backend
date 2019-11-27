using Core.GraphSupport.Model;
using Core.ReadModel.Network;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RouteNetwork.ReadModel
{
    public sealed class RouteSegmentInfo : GraphEdge, IRouteElementInfo, ISegment, INetworkElement
    {
        public Guid Id { get; set; }
        public Guid FromRouteNodeId { get; set; }
        public Guid ToRouteNodeId { get; set; }
        public string Name { get; set; }
        public RouteSegmentKindEnum SegmentKind { get; set; }
        public Geometry Geometry { get; set; }

        public LineKindEnum LineKind
        {
            get
            {
                return LineKindEnum.Route;
            }
        }

        public SegmentRelationTypeEnum RelationType(Guid pointOfInterestId)
        {
            if (FromNodeId == pointOfInterestId)
                return SegmentRelationTypeEnum.Incomming;
            else if (ToNodeId == pointOfInterestId)
                return SegmentRelationTypeEnum.Outgoing;
            else
                return SegmentRelationTypeEnum.NotRelated;
        }

        [IgnoreDataMember]
        public double Length { get; set; }

        [IgnoreDataMember]
        public INode FromRouteNode { get; set; }

        [IgnoreDataMember]
        public INode ToRouteNode { get; set; }

        [IgnoreDataMember]
        public Guid FromNodeId { get { return FromRouteNodeId; } set { FromRouteNodeId = value; } }

        [IgnoreDataMember]
        public Guid ToNodeId { get { return ToRouteNodeId; } set { ToRouteNodeId = value; } }

        [IgnoreDataMember]
        public INode FromNode { get { return FromRouteNode; } set { FromRouteNode = (RouteNodeInfo)value; } }

        [IgnoreDataMember]
        public INode ToNode { get { return ToRouteNode; } set { ToRouteNode = (RouteNodeInfo)value; } }

        [IgnoreDataMember]
        private List<WalkOfInterestInfo> _walkOfInterests { get; set; }

        public void AddWalkOfInterest(WalkOfInterestInfo walkOfInterest)
        {
            if (_walkOfInterests == null)
                _walkOfInterests = new List<WalkOfInterestInfo>();

            _walkOfInterests.Add(walkOfInterest);
        }

        [IgnoreDataMember]
        public override List<IGraphElement> OutgoingElements
        {
            get
            {
                return new List<IGraphElement>() { ToRouteNode };
            }
        }

        [IgnoreDataMember]
        public override List<IGraphElement> IngoingElements
        {
            get
            {
                return new List<IGraphElement>() { FromRouteNode };
            }
        }

        [IgnoreDataMember]
        public override List<IGraphElement> NeighborElements
        {
            get
            {
                return new List<IGraphElement>() { ToRouteNode, FromRouteNode };
            }
        }

        [IgnoreDataMember]
        public List<WalkOfInterestInfo> WalkOfInterests
        {
            get
            {
                if (_walkOfInterests != null)
                    return _walkOfInterests;
                else

                    return new List<WalkOfInterestInfo>();
            }
        }

        [IgnoreDataMember]
        public LineKindEnum LineSegmentKind => LineKindEnum.Route;

        [IgnoreDataMember]
        public ILine Line { get => null; set { } }

        [IgnoreDataMember]
        public List<ISegment> Parents { get => null; set { } }

        [IgnoreDataMember]
        public List<ISegment> Children { get => null; set { } }

        [IgnoreDataMember]
        public int SequenceNumber { get => 1; set { } }
              

        public override string ToString()
        {
            return "RouteSegment (" + FromRouteNode.Name + " -> " + ToRouteNode.Name + ")";
        }
    }
}
