using Core.GraphSupport.Model;
using Core.ReadModel.Network;
using Location.Model;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace RouteNetwork.ReadModel
{
    public sealed class RouteNodeInfo : GraphNode, IRouteElementInfo, INode, INetworkElement
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public RouteNodeKindEnum NodeKind { get; set; }
        public RouteNodeFunctionKindEnum NodeFunctionKind { get; set; }
        public Geometry Geometry { get; set; }

        public LocationInfo LocationInfo { get; set; }

        [IgnoreDataMember]
        private List<RouteSegmentInfo> _ingoingSegments { get; set; }

        [IgnoreDataMember]
        private List<RouteSegmentInfo> _outgoingSegments { get; set; }

        [IgnoreDataMember]
        private List<WalkOfInterestInfo> _walkOfInterests { get; set; }

        public void AddWalkOfInterest(WalkOfInterestInfo walkOfInterest)
        {
            if (_walkOfInterests == null)
                _walkOfInterests = new List<WalkOfInterestInfo>();

            _walkOfInterests.Add(walkOfInterest);
        }

        public void AddIngoingSegment(RouteSegmentInfo segment)
        {
            if (_ingoingSegments == null)
                _ingoingSegments = new List<RouteSegmentInfo>();

            if (_ingoingSegments.Contains(segment))
                throw new ArgumentException("Segment: " + segment.Id + " already connected to node: " + this.Id);

            _ingoingSegments.Add(segment);
        }

        public void AddOutgoingSegment(RouteSegmentInfo segment)
        {
            if (_outgoingSegments == null)
                _outgoingSegments = new List<RouteSegmentInfo>();

            if (_outgoingSegments.Contains(segment))
                throw new ArgumentException("Segment: " + segment.Id + " already connected to node: " + this.Id);

            _outgoingSegments.Add(segment);
        }

        [IgnoreDataMember]
        public List<RouteSegmentInfo> IngoingSegments
        {
            get
            {
                if (_ingoingSegments != null)
                    return _ingoingSegments;
                else
                    return new List<RouteSegmentInfo>();
            }
        }

        [IgnoreDataMember]
        public List<RouteSegmentInfo> OutgoingSegments
        {
            get
            {
                if (_outgoingSegments != null)
                    return _outgoingSegments;
                else
                    return new List<RouteSegmentInfo>();
            }
        }

        [IgnoreDataMember]
        public override List<IGraphElement> IngoingElements
        {
            get
            {
                return IngoingSegments.ToList<IGraphElement>();
            }
        }

        [IgnoreDataMember]
        public override List<IGraphElement> OutgoingElements
        {
            get
            {
                return OutgoingSegments.ToList<IGraphElement>();
            }
        }

        [IgnoreDataMember]
        public override List<IGraphElement> NeighborElements
        {
            get
            {
                var neighbors = new List<IGraphElement>();

                neighbors.AddRange(OutgoingElements);
                neighbors.AddRange(IngoingElements);

                return neighbors;
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

        public override string ToString()
        {
            return "RouteNode (" + Name + ")";
        }
    }
}
