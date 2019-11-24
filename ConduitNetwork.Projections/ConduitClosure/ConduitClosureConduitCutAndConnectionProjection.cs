using ConduitNetwork.Events;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel;
using ConduitNetwork.ReadModel.ConduitClosure;
using Core.GraphSupport.Model;
using Marten.Events.Projections;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConduitNetwork.Projections.ConduitClosure
{
    /// <summary>
    /// Handles the different conduit cut and connection events that effects the conduit closure read model
    /// </summary>
    public sealed class ConduitClosureConduitCutAndConnectionProjection : ViewProjection<ConduitClosureInfo, Guid>
    {
        private IRouteNetworkState routeNetworkQueryService = null;
        private IConduitNetworkQueryService conduitNetworkQueryService = null;
        private IConduitClosureRepository conduitClosureRepository = null;

        public ConduitClosureConduitCutAndConnectionProjection(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IConduitClosureRepository conduitClosureRepository)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkQueryService = conduitNetworkQueryService;
            this.conduitClosureRepository = conduitClosureRepository;

            // Outer conduit cut
            ProjectEvent<MultiConduitOuterConduitCut>((session, e) =>
            {
                // Try find conduit closure affected by cut
                if (conduitClosureRepository.CheckIfRouteNodeContainsConduitClosure(e.PointOfInterestId))
                {
                    var conduitClosure = conduitClosureRepository.GetConduitClosureInfoByRouteNodeId(e.PointOfInterestId);

                    if (conduitClosure.Sides.Exists(s => s.Ports.Exists(p => p.MultiConduitId == e.MultiConduitId)))
                    {
                        return conduitClosure.Id;
                    }
                }

                return Guid.Empty;
            },
            OnMultiConduitOuterConduitCut);


            // Inner conduit cut
            ProjectEvent<MultiConduitInnerConduitCut>((session, e) =>
            {
                // Try find conduit closure affected by cut
                if (conduitClosureRepository.CheckIfRouteNodeContainsConduitClosure(e.PointOfInterestId))
                {
                    var conduitClosure = conduitClosureRepository.GetConduitClosureInfoByRouteNodeId(e.PointOfInterestId);

                    if (conduitClosure.Sides.Exists(s => s.Ports.Exists(p => p.MultiConduitId == e.MultiConduitId)))
                    {
                        return conduitClosure.Id;
                    }
                }

                return Guid.Empty;
            },
            OnMultiConduitInnerConduitCut);

            // Single conduit connected
            ProjectEvent<SingleConduitConnected>((session, e) =>
            {
                // Try find conduit closure affected by connection
                if (conduitClosureRepository.CheckIfRouteNodeContainsConduitClosure(e.PointOfInterestId))
                {
                    var conduitClosure = conduitClosureRepository.GetConduitClosureInfoByRouteNodeId(e.PointOfInterestId);

                    if (conduitClosure.Sides.Exists(s => s.Ports.Exists(p => p.Terminals.Exists(t => t.LineId == e.SingleConduitId))))
                    {
                        return conduitClosure.Id;
                    }
                }

                return Guid.Empty;
            },
            OnSingleConduitConnected);

            // Inner conduit connected
            ProjectEvent<MultiConduitInnerConduitConnected>((session, e) =>
            {
                // Try find conduit closure affected by connection
                if (conduitClosureRepository.CheckIfRouteNodeContainsConduitClosure(e.PointOfInterestId))
                {
                    var conduitClosure = conduitClosureRepository.GetConduitClosureInfoByRouteNodeId(e.PointOfInterestId);

                    if (conduitClosure.Sides.Exists(s => s.Ports.Exists(p => p.MultiConduitId == e.MultiConduitId)))
                    {
                        return conduitClosure.Id;
                    }
                }

                return Guid.Empty;
            },
            OnInnerConduitConnected);
        }

        private void OnSingleConduitConnected(ConduitClosureInfo conduitClosure, SingleConduitConnected @event)
        {
            // Get single conduit
            var singleConduit = conduitNetworkQueryService.GetSingleConduitInfo(@event.SingleConduitId);

            ConnectTerminals(conduitClosure, singleConduit, @event.PointOfInterestId, @event.ConnectedJunctionId, @event.ConnectedEndKind);

            conduitClosureRepository.UpdateConduitClosureInfo(conduitClosure);
        }

        private void OnInnerConduitConnected(ConduitClosureInfo conduitClosure, MultiConduitInnerConduitConnected @event)
        {
            // Get multi conduit
            var multiConduit = conduitNetworkQueryService.GetMultiConduitInfo(@event.MultiConduitId);

            // Get the inner conduit
            var innerConduit = multiConduit.Children.OfType<ConduitInfo>().Single(c => c.SequenceNumber == @event.InnerConduitSequenceNumber);

            ConnectTerminals(conduitClosure, innerConduit, @event.PointOfInterestId, @event.ConnectedJunctionId, @event.ConnectedEndKind);

            conduitClosureRepository.UpdateConduitClosureInfo(conduitClosure);
        }

        private void ConnectTerminals(ConduitClosureInfo conduitClosure, ConduitInfo singleConduit, Guid pointOfInterestId, Guid junctionId, ConduitEndKindEnum endKind)
        {
            var relatedSegments = FindRelatedSegmentInfo(singleConduit, pointOfInterestId, endKind);

            if (relatedSegments.Count == 1)
            {
                var segment = relatedSegments[0].Segment;

                // Check if terminal with such segment exists
                if (conduitClosure.Sides.Exists(s => s.Ports.Exists(p => p.Terminals.Exists(t => t.LineSegmentId == segment.Id))))
                {
                    var side = conduitClosure.Sides.Find(s => s.Ports.Exists(p => p.Terminals.Exists(t => t.LineSegmentId == segment.Id)));
                    var port = side.Ports.Find(p => p.Terminals.Exists(t => t.LineSegmentId == segment.Id));
                    var terminal = port.Terminals.Find(t => t.LineSegmentId == segment.Id);

                    ConduitSegmentInfo connectedSegment = null;

                    // Get the segment on the other side of the junction
                    if (segment.FromNodeId == junctionId && segment.FromNode.NeighborElements.OfType<ConduitSegmentInfo>().Any(n => n != segment))
                        connectedSegment = segment.FromNode.NeighborElements.OfType<ConduitSegmentInfo>().First(n => n != segment);
                    else if (segment.ToNodeId == junctionId && segment.ToNode.NeighborElements.OfType<ConduitSegmentInfo>().Any(n => n != segment))
                        connectedSegment = segment.ToNode.NeighborElements.OfType<ConduitSegmentInfo>().First(n => n != segment);

                    // Check if terminal with related segment exisits
                    if (connectedSegment != null && conduitClosure.Sides.Exists(s => s.Ports.Exists(p => p.Terminals.Exists(t => t.LineSegmentId == connectedSegment.Id))))
                    {
                        var connectedSide = conduitClosure.Sides.Find(s => s.Ports.Exists(p => p.Terminals.Exists(t => t.LineSegmentId == connectedSegment.Id)));
                        var connectedPort = connectedSide.Ports.Find(p => p.Terminals.Exists(t => t.LineSegmentId == connectedSegment.Id));
                        var connectedTerminal = connectedPort.Terminals.Find(t => t.LineSegmentId == connectedSegment.Id);

                        // Okay, we're safe to connect the two terminals together
                        terminal.ConnectionKind = ConduitClosureInternalConnectionKindEnum.Connected;
                        terminal.ConnectedToSide = connectedSide.Position;
                        terminal.ConnectedToPort = connectedPort.Position;
                        terminal.ConnectedToTerminal = connectedTerminal.Position;

                        connectedTerminal.ConnectionKind = ConduitClosureInternalConnectionKindEnum.Connected;
                        connectedTerminal.ConnectedToSide = side.Position;
                        connectedTerminal.ConnectedToPort = port.Position;
                        connectedTerminal.ConnectedToTerminal = terminal.Position;
                    }
                }
            }
        }

        private void OnMultiConduitOuterConduitCut(ConduitClosureInfo conduitClosure, MultiConduitOuterConduitCut @event)
        {
            // Get multi conduit
            var multiConduit = conduitNetworkQueryService.GetMultiConduitInfo(@event.MultiConduitId);

            var relatedSegments = FindRelatedSegmentInfo(multiConduit, @event.PointOfInterestId);

            if (relatedSegments.Exists(s => s.RelationType == ConduitRelationTypeEnum.Incomming))
            {
                var incommingSegment = relatedSegments.Find(s => s.RelationType == ConduitRelationTypeEnum.Incomming);
                
                // Find incomming port
                if (conduitClosure.Sides.Exists(s => s.Ports.Exists(p => p.MultiConduitId == @event.MultiConduitId && p.MultiConduitSegmentEndKind == ConduitEndKindEnum.Incomming)))
                {
                    var side = conduitClosure.Sides.Find(s => s.Ports.Exists(p => p.MultiConduitId == @event.MultiConduitId && p.MultiConduitSegmentEndKind == ConduitEndKindEnum.Incomming));
                    var port =  side.Ports.Find(p => p.MultiConduitId == @event.MultiConduitId && p.MultiConduitSegmentEndKind == ConduitEndKindEnum.Incomming);

                    port.ConnectionKind = ConduitClosureInternalConnectionKindEnum.NotConnected;
                    port.ConnectedToSide = 0;
                    port.ConnectedToPort = 0;
                    port.MultiConduitSegmentId = incommingSegment.Segment.Id;
                    port.MultiConduitSegment = null; // to force re-resolving
                }
            }

            if (relatedSegments.Exists(s => s.RelationType == ConduitRelationTypeEnum.Outgoing))
            {
                var incommingSegment = relatedSegments.Find(s => s.RelationType == ConduitRelationTypeEnum.Outgoing);

                // Find incomming port
                if (conduitClosure.Sides.Exists(s => s.Ports.Exists(p => p.MultiConduitId == @event.MultiConduitId && p.MultiConduitSegmentEndKind == ConduitEndKindEnum.Outgoing)))
                {
                    var side = conduitClosure.Sides.Find(s => s.Ports.Exists(p => p.MultiConduitId == @event.MultiConduitId && p.MultiConduitSegmentEndKind == ConduitEndKindEnum.Outgoing));
                    var port = side.Ports.Find(p => p.MultiConduitId == @event.MultiConduitId && p.MultiConduitSegmentEndKind == ConduitEndKindEnum.Outgoing);

                    port.ConnectionKind = ConduitClosureInternalConnectionKindEnum.NotConnected;
                    port.ConnectedToSide = 0;
                    port.ConnectedToPort = 0;
                    port.MultiConduitSegmentId = incommingSegment.Segment.Id;
                    port.MultiConduitSegment = null; // to force re-resolving
                }
            }

            conduitClosureRepository.UpdateConduitClosureInfo(conduitClosure);

        }

        private void OnMultiConduitInnerConduitCut(ConduitClosureInfo conduitClosure, MultiConduitInnerConduitCut @event)
        {
            // Get multi conduit
            var multiConduit = conduitNetworkQueryService.GetMultiConduitInfo(@event.MultiConduitId);

            // Get the inner conduit that is cut
            var innerConduit = multiConduit.Children.OfType<ConduitInfo>().Single(c => c.SequenceNumber == @event.InnerConduitSequenceNumber);

            var relatedSegments = FindRelatedSegmentInfo(innerConduit, @event.PointOfInterestId);

            if (relatedSegments.Exists(s => s.RelationType == ConduitRelationTypeEnum.Incomming))
            {
                var incommingSegment = relatedSegments.Find(s => s.RelationType == ConduitRelationTypeEnum.Incomming);

                // Find incomming terminal
                if (conduitClosure.Sides.Exists(s => s.Ports.Exists(p => p.MultiConduitId == @event.MultiConduitId && p.MultiConduitSegmentEndKind == ConduitEndKindEnum.Incomming)))
                {
                    var side = conduitClosure.Sides.Find(s => s.Ports.Exists(p => p.MultiConduitId == @event.MultiConduitId && p.MultiConduitSegmentEndKind == ConduitEndKindEnum.Incomming));
                    var port = side.Ports.Find(p => p.MultiConduitId == @event.MultiConduitId && p.MultiConduitSegmentEndKind == ConduitEndKindEnum.Incomming);

                    if (port.Terminals.Exists(t => t.Position == @event.InnerConduitSequenceNumber))
                    {
                        var terminal = port.Terminals.Find(t => t.Position == @event.InnerConduitSequenceNumber);
                        terminal.ConnectionKind = ConduitClosureInternalConnectionKindEnum.NotConnected;
                        terminal.ConnectedToSide = 0;
                        terminal.ConnectedToPort = 0;
                        terminal.ConnectedToTerminal = 0;
                        terminal.LineSegmentId = incommingSegment.Segment.Id;
                        terminal.LineSegment = null; // to force re-resolving
                    }
                }
            }

            if (relatedSegments.Exists(s => s.RelationType == ConduitRelationTypeEnum.Outgoing))
            {
                var outgoingSegment = relatedSegments.Find(s => s.RelationType == ConduitRelationTypeEnum.Outgoing);

                // Find incomming terminal
                if (conduitClosure.Sides.Exists(s => s.Ports.Exists(p => p.MultiConduitId == @event.MultiConduitId && p.MultiConduitSegmentEndKind == ConduitEndKindEnum.Outgoing)))
                {
                    var side = conduitClosure.Sides.Find(s => s.Ports.Exists(p => p.MultiConduitId == @event.MultiConduitId && p.MultiConduitSegmentEndKind == ConduitEndKindEnum.Outgoing));
                    var port = side.Ports.Find(p => p.MultiConduitId == @event.MultiConduitId && p.MultiConduitSegmentEndKind == ConduitEndKindEnum.Outgoing);

                    if (port.Terminals.Exists(t => t.Position == @event.InnerConduitSequenceNumber))
                    {
                        var terminal = port.Terminals.Find(t => t.Position == @event.InnerConduitSequenceNumber);
                        terminal.ConnectionKind = ConduitClosureInternalConnectionKindEnum.NotConnected;
                        terminal.ConnectedToSide = 0;
                        terminal.ConnectedToPort = 0;
                        terminal.ConnectedToTerminal = 0;
                        terminal.LineSegmentId = outgoingSegment.Segment.Id;
                        terminal.LineSegment = null; // to force re-resolving
                    }
                }
            }

            conduitClosureRepository.UpdateConduitClosureInfo(conduitClosure);
        }


        private RelatedConduitClosurePortInfo FindRelatedTerminal(ConduitClosureInfo conduitClosureInfo, Guid singleConduitSegmentId)
        {
            RelatedConduitClosurePortInfo result = new RelatedConduitClosurePortInfo();

            // First try to see if we can find an existing port having that conduit segment attached. If so, it's a pass through
            foreach (var side in conduitClosureInfo.Sides)
            {
                result.Side = side.Position;

                foreach (var port in side.Ports)
                {
                    result.Port = port.Position;

                    foreach (var terminal in port.Terminals)
                    {
                        result.Terminal = terminal.Position;

                        if (terminal.LineSegmentId == singleConduitSegmentId)
                        {
                            result.ConnectionKind = ConduitClosureInternalConnectionKindEnum.PassThrough;
                            return result;
                        }
                    }
                }
            }

            // Now try if we can find a port with a connected segment
            foreach (var side in conduitClosureInfo.Sides)
            {
                result.Side = side.Position;

                foreach (var port in side.Ports)
                {
                    result.Port = port.Position;

                    foreach (var terminal in port.Terminals)
                    {
                        if (terminal.LineSegment != null)
                        {
                            foreach (var junction in ((IGraphElement)terminal.LineSegment).NeighborElements)
                            {
                                if (junction.NeighborElements.OfType<ConduitSegmentInfo>().ToList().Exists(n => n.Id == singleConduitSegmentId))
                                {
                                    result.ConnectionKind = ConduitClosureInternalConnectionKindEnum.Connected;
                                    return result;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Helper function to find related segment
        /// </summary>
        /// <param name="conduitId"></param>
        /// <param name="pointOfInterestId"></param>
        /// <returns></returns>
        private List<RelatedSingleConduitSegmentInfo> FindRelatedSegmentInfo(ConduitInfo conduit, Guid pointOfInterestId, ConduitEndKindEnum? conduitEndKind = null)
        {
            List<RelatedSingleConduitSegmentInfo> result = new List<RelatedSingleConduitSegmentInfo>();

            var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(conduit.GetRootConduit().WalkOfInterestId);

            foreach (var existingSegment in conduit.Segments.OfType<ConduitSegmentInfo>())
            {
                var segmentWalk = walkOfInterest.SubWalk2(existingSegment.FromRouteNodeId, existingSegment.ToRouteNodeId);

                if (segmentWalk.StartNodeId == pointOfInterestId)
                {
                    if (conduitEndKind == null || conduitEndKind.Value == ConduitEndKindEnum.Outgoing)
                    {
                        result.Add(
                            new RelatedSingleConduitSegmentInfo()
                            {
                                Segment = existingSegment,
                                RelationType = ConduitRelationTypeEnum.Outgoing
                            }
                        );
                    }
                }

                if (segmentWalk.EndNodeId == pointOfInterestId)
                {
                    if (conduitEndKind == null || conduitEndKind.Value == ConduitEndKindEnum.Incomming)
                    {
                        result.Add(
                            new RelatedSingleConduitSegmentInfo()
                            {
                                Segment = existingSegment,
                                RelationType = ConduitRelationTypeEnum.Incomming
                            }
                        );
                    }
                }

                if (result.Count == 0 && segmentWalk.AllNodeIds.Contains(pointOfInterestId))
                {
                    if (conduitEndKind == null || conduitEndKind.Value == ConduitEndKindEnum.Incomming)
                    {
                        result.Add(
                            new RelatedSingleConduitSegmentInfo()
                            {
                                Segment = existingSegment,
                                RelationType = ConduitRelationTypeEnum.PassThrough
                            }
                        );
                    }
                }
            }

            return result;
        }

        private class RelatedSingleConduitSegmentInfo
        {
            public ConduitSegmentInfo Segment { get; set; }
            public ConduitRelationTypeEnum RelationType { get; set; }
        }

        private class RelatedConduitClosurePortInfo
        {
            public ConduitClosureInfoSide Side { get; set; }
            public int Port { get; set; }
            public int Terminal { get; set; }
            public ConduitClosureInternalConnectionKindEnum ConnectionKind { get; set; }
        }
    }
}
