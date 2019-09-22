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
    /// Handles the different conduit attached events
    /// </summary>
    public sealed class ConduitClosureAttachmentProjection : ViewProjection<ConduitClosureInfo, Guid>
    {
        private IRouteNetworkQueryService routeNetworkQueryService = null;
        private IConduitNetworkQueryService conduitNetworkQueryService = null;
        private IConduitClosureRepository conduitClosureRepository = null;

        public ConduitClosureAttachmentProjection(IRouteNetworkQueryService routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IConduitClosureRepository conduitClosureRepository)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkQueryService = conduitNetworkQueryService;
            this.conduitClosureRepository = conduitClosureRepository;

            // Conduit passing by attached to closure
            ProjectEvent<ConduitClosurePassingByConduitAttached>(
            (session, multiConduitEndAttached) =>
            {
                return multiConduitEndAttached.ConduitClosureId;
            },
            OnConduitClosurePassingByConduitAttached);

            // Conduit end attached to closure
            ProjectEvent<ConduitClosureConduitEndAttached>(
            (session, e) =>
            {
                return e.ConduitClosureId;
            },
            OnConduitClosureConduitEndAttached);

        }

        private void OnConduitClosurePassingByConduitAttached(ConduitClosureInfo conduitClosureInfo, ConduitClosurePassingByConduitAttached @event)
        {
            // Get conduit
            var conduit = conduitNetworkQueryService.GetConduitInfo(@event.ConduitId);

            if (conduit is MultiConduitInfo)
            {
                // Incomming side
                AttachMultiConduitToPort(conduitClosureInfo, @event, @event.IncommingSide);
                AttachInnerConduitsToTerminals(conduitClosureInfo, @event, @event.IncommingSide);

                // Outgoing side
                AttachMultiConduitToPort(conduitClosureInfo, @event, @event.OutgoingSide);
                AttachInnerConduitsToTerminals(conduitClosureInfo, @event, @event.OutgoingSide);

                conduitClosureRepository.UpdateConduitClosureInfo(conduitClosureInfo);
            }
        }

        private void OnConduitClosureConduitEndAttached(ConduitClosureInfo conduitClosure, ConduitClosureConduitEndAttached @event)
        {
            // Get conduit
            var conduit = conduitNetworkQueryService.GetConduitInfo(@event.ConduitId);

            if (conduit is MultiConduitInfo)
            {
                //AttachMultiConduitToPort(conduitClosureInfo, @event, @event.Side);
                //AttachInnerConduitsToTerminals(conduitClosureInfo, @event, @event.IncommingSide);
            }
            else if (conduit is SingleConduitInfo)
            {
                AttachSingleConduitToPortTerminal(conduitClosure, @event);
                conduitClosureRepository.UpdateConduitClosureInfo(conduitClosure);
            }
        }

        private void AttachSingleConduitToPortTerminal(ConduitClosureInfo conduitClosureInfo, ConduitClosureConduitEndAttached @event)
        {
            // Get conduit
            var conduit = conduitNetworkQueryService.GetConduitInfo(@event.ConduitId);

            // Find the conduit segment that is related to the point of interest of the conduit closure
            var relatedSegmentInfo = FindRelatedSegmentInfo(conduit, conduitClosureInfo.PointOfInterestId);

            var side = conduitClosureInfo.Sides.Find(s => s.Position == @event.Side);
            var portPosition = @event.PortPosition;
            var endKind = relatedSegmentInfo.RelationType == ConduitRelationTypeEnum.Incomming ? ConduitEndKindEnum.Incomming : ConduitEndKindEnum.Outgoing;

            // Create port
            var newPort = new ConduitClosurePortInfo()
            {
                Position = portPosition,
                ConnectionKind = ConduitClosureInternalConnectionKindEnum.NotConnected
            };


            // Create terminal
            var newTerminal = new ConduitClosureTerminalInfo()
            {
                Position = @event.TerminalPosition,
                ConnectionKind = ConduitClosureInternalConnectionKindEnum.NotConnected,
                LineId = @event.ConduitId,
                LineSegmentId = relatedSegmentInfo.Segment.Id,
                LineSegmentEndKind = endKind
            };

            newPort.Terminals.Add(newTerminal);

            side.Ports.Add(newPort);

        }


        private ConduitClosurePortInfo AttachMultiConduitToPort(ConduitClosureInfo conduitClosureInfo, ConduitClosurePassingByConduitAttached @event, ConduitClosureSideEnum sidePosition)
        {
            // Get conduit
            var conduit = conduitNetworkQueryService.GetConduitInfo(@event.ConduitId);

            // Get conduit end kind (to be placed on terminal)
            var endKind = @event.IncommingSide == sidePosition ? ConduitEndKindEnum.Incomming : ConduitEndKindEnum.Outgoing;

            // Find the conduit segment that is related to the point of interest of the conduit closure
            var relatedSegmentInfo = FindRelatedSegmentInfo(conduit, conduitClosureInfo.PointOfInterestId);

            var side = conduitClosureInfo.Sides.Find(s => s.Position == sidePosition);
            var portPosition = @event.IncommingSide == sidePosition ? @event.IncommingPortPosition : @event.OutgoingPortPosition;

            // Create port
            var newPort = new ConduitClosurePortInfo()
            {
                Position = portPosition,
                ConnectionKind = ConduitClosureInternalConnectionKindEnum.NotConnected,
                MultiConduitId = conduit.Id,
                MultiConduitSegmentId = relatedSegmentInfo.Segment.Id,
                MultiConduitSegmentEndKind = endKind
            };

            // Try find other side
            var otherSidePort = FindRelatedPort(conduitClosureInfo, relatedSegmentInfo.Segment.Id);

            if (otherSidePort != null)
            {
                newPort.ConnectedToSide = otherSidePort.Side;
                newPort.ConnectedToPort = otherSidePort.Port;
                newPort.ConnectionKind = otherSidePort.ConnectionKind;

                // Update the other end as well
                var otherEndPort = conduitClosureInfo.Sides.Find(s => s.Position == otherSidePort.Side).Ports.Find(p => p.Position == otherSidePort.Port);
                otherEndPort.ConnectedToSide = sidePosition;
                otherEndPort.ConnectedToPort = portPosition;
                otherEndPort.ConnectionKind = otherSidePort.ConnectionKind;
            }

            side.Ports.Add(newPort);

            return newPort;
        }

        private void AttachInnerConduitsToTerminals(ConduitClosureInfo conduitClosureInfo, ConduitClosurePassingByConduitAttached @event, ConduitClosureSideEnum sidePosition)
        {
            // Get side
            var side = conduitClosureInfo.Sides.Find(s => s.Position == sidePosition);

            // Get conduit end kind (to be placed on terminal)
            var endKind = @event.IncommingSide == sidePosition ? ConduitEndKindEnum.Incomming : ConduitEndKindEnum.Outgoing;

            // Get port position
            var portPosition = @event.IncommingSide == sidePosition ? @event.IncommingPortPosition : @event.OutgoingPortPosition;

            // Get port
            var port = side.Ports.Find(p => p.Position == portPosition);

            // Get multi conduit
            var multiConduit = conduitNetworkQueryService.GetConduitInfo(@event.ConduitId);

            int terminalPosition = 1;

            foreach (var innerConduit in multiConduit.Children)
            {
                // Get related segment info
                var relatedSegmentInfo = FindRelatedSegmentInfo(innerConduit, conduitClosureInfo.PointOfInterestId);

                // Create terminal
                var newTerminal = new ConduitClosureTerminalInfo()
                {
                    Position = terminalPosition,
                    ConnectionKind = ConduitClosureInternalConnectionKindEnum.NotConnected,
                    LineId = innerConduit.Id,
                    LineSegmentId = relatedSegmentInfo.Segment.Id,
                    LineSegmentEndKind = endKind
                };

                // Try find other side
                var otherEndTerminalInfo = FindRelatedTerminal(conduitClosureInfo, relatedSegmentInfo.Segment.Id);

                if (otherEndTerminalInfo != null)
                {
                    newTerminal.ConnectedToSide = otherEndTerminalInfo.Side;
                    newTerminal.ConnectedToPort = otherEndTerminalInfo.Port;
                    newTerminal.ConnectedToTerminal = otherEndTerminalInfo.Terminal;
                    newTerminal.ConnectionKind = otherEndTerminalInfo.ConnectionKind;

                    // Update the other end as well
                    var otherEndPort = conduitClosureInfo.Sides.Find(s => s.Position == otherEndTerminalInfo.Side).Ports.Find(p => p.Position == otherEndTerminalInfo.Port).Terminals.Find(t => t.Position == otherEndTerminalInfo.Terminal);
                    otherEndPort.ConnectedToSide = sidePosition;
                    otherEndPort.ConnectedToPort = portPosition;
                    otherEndPort.ConnectedToTerminal = terminalPosition;
                    otherEndPort.ConnectionKind = otherEndTerminalInfo.ConnectionKind;
                }

                port.Terminals.Add(newTerminal);

                terminalPosition++;
            }
        }

        private RelatedConduitClosurePortInfo FindRelatedPort(ConduitClosureInfo conduitClosureInfo, Guid multiConduitSegmentId)
        {
            RelatedConduitClosurePortInfo result = new RelatedConduitClosurePortInfo();

            // First try to see if we can find an existing port having that conduit segment attached. If so, it's a pass through
            foreach (var side in conduitClosureInfo.Sides)
            {
                result.Side = side.Position;

                foreach (var port in side.Ports)
                {
                    result.Port = port.Position;

                    if (port.MultiConduitSegmentId == multiConduitSegmentId)
                    {
                        result.ConnectionKind = ConduitClosureInternalConnectionKindEnum.PassThrough;
                        return result;
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

                    if (port.MultiConduitSegment != null)
                    {
                        foreach (var junction in port.MultiConduitSegment.NeighborElements)
                        {
                            if (junction.NeighborElements.OfType<ConduitSegmentInfo>().ToList().Exists(n => n.Id == multiConduitSegmentId))
                            {
                                result.ConnectionKind = ConduitClosureInternalConnectionKindEnum.Connected;
                                return result;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private RelatedConduitClosureTerminalInfo FindRelatedTerminal(ConduitClosureInfo conduitClosureInfo, Guid singleConduitSegmentId)
        {
            RelatedConduitClosureTerminalInfo result = new RelatedConduitClosureTerminalInfo();

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
        /// <param name="multiConduitId"></param>
        /// <param name="pointOfInterestId"></param>
        /// <returns></returns>
        private RelatedMultiConduitSegmentInfo FindRelatedSegmentInfo(ConduitInfo conduit, Guid pointOfInterestId)
        {
            var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(conduit.GetRootConduit().WalkOfInterestId);

            foreach (var existingSegment in conduit.Segments)
            {
                var segmentWalk = walkOfInterest.SubWalk2(existingSegment.FromNodeId, existingSegment.ToNodeId);

                if (segmentWalk.StartNodeId == pointOfInterestId)
                {
                    return new RelatedMultiConduitSegmentInfo()
                    {
                        Segment = existingSegment,
                        RelationType = ConduitRelationTypeEnum.Outgoing
                    };
                }
                else if (segmentWalk.EndNodeId == pointOfInterestId)
                {
                    return new RelatedMultiConduitSegmentInfo()
                    {
                        Segment = existingSegment,
                        RelationType = ConduitRelationTypeEnum.Incomming
                    };
                }
                else if (segmentWalk.AllNodeIds.Contains(pointOfInterestId))
                {
                    return new RelatedMultiConduitSegmentInfo()
                    {
                        Segment = existingSegment,
                        RelationType = ConduitRelationTypeEnum.PassThrough
                    };
                }
            }

            return null;
        }

        private class RelatedMultiConduitSegmentInfo
        {
            public ConduitSegmentInfo Segment { get; set; }
            public ConduitRelationTypeEnum RelationType { get; set; }
        }

        private class RelatedConduitClosurePortInfo
        {
            public ConduitClosureSideEnum Side { get; set; }
            public int Port { get; set; }
            public ConduitClosureInternalConnectionKindEnum ConnectionKind { get; set; }
        }

        private class RelatedConduitClosureTerminalInfo
        {
            public ConduitClosureSideEnum Side { get; set; }
            public int Port { get; set; }
            public int Terminal { get; set; }
            public ConduitClosureInternalConnectionKindEnum ConnectionKind { get; set; }
        }

    }
}
