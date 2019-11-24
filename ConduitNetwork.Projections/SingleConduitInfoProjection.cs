using ConduitNetwork.Events;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.ReadModel;
using Core.ReadModel.Network;
using Marten.Events.Projections;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConduitNetwork.Projections
{
    public sealed class SingleConduitInfoProjection : ViewProjection<SingleConduitInfo, Guid>
    {
        private IRouteNetworkState routeNetworkQueryService = null;
        private ConduitNetworkQueryService conduitNetworkQueryService = null;

        public SingleConduitInfoProjection(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkQueryService = (ConduitNetworkQueryService)conduitNetworkQueryService;
            

            // New single conduits
            ProjectEvent<SingleConduitPlaced>(
            (session, singleConduitPlaced) =>
            {
                return singleConduitPlaced.SingleConduitId;
            },
            SingleConduitPlaced);

            // Single conduits connected
            ProjectEvent<SingleConduitConnected>(
            (session, singleConduitConnected) =>
            {
                return singleConduitConnected.SingleConduitId;
            },
            SingleConduitConnected);


            // We also want to update single conduit info when a inner conduit of a multi conduit is added
            ProjectEvent<MultiConduitInnerConduitAdded>(
            (session, innerConduitAdded) =>
            {
                return innerConduitAdded.ConduitInfo.Id;
            },
            InnerConduitAdded);

            
            // We also want to update the single conduit info when a inner conduit of a multi conduit is cut
            ProjectEvent<MultiConduitInnerConduitCut>(
            (session, innerConduitAdded) =>
            {
                // To get to the right single conduit, we need look it up through the multi conduit
                var mutltiConduitInfo = this.conduitNetworkQueryService.GetMultiConduitInfo(innerConduitAdded.MultiConduitId);
                return mutltiConduitInfo.Children.OfType<ConduitInfo>().Single(c => c.SequenceNumber == innerConduitAdded.InnerConduitSequenceNumber).Id;
            },
            InnerConduitCut);
            

            // We also want to update the single conduit info when a inner conduit of a multi conduit is connected
            ProjectEvent<MultiConduitInnerConduitConnected>(
            (session, innerConduitConnected) =>
            {
                // To get to the right single conduit, we need look it up through the multi conduit
                var mutltiConduitInfo = this.conduitNetworkQueryService.GetMultiConduitInfo(innerConduitConnected.MultiConduitId);
                return mutltiConduitInfo.Children.OfType<ConduitInfo>().Single(c => c.SequenceNumber == innerConduitConnected.InnerConduitSequenceNumber).Id;
            },
            InnerConduitConnected);
        }

        private void SingleConduitPlaced(SingleConduitInfo singleConduitInfo, SingleConduitPlaced @event)
        {
            singleConduitInfo.Id = @event.SingleConduitId;
            singleConduitInfo.Kind = ConduitKindEnum.SingleConduit;
            singleConduitInfo.AssetInfo = @event.AssetInfo;
            singleConduitInfo.WalkOfInterestId = @event.WalkOfInterestId;
            singleConduitInfo.Color = @event.ConduitInfo.Color;
            singleConduitInfo.Shape = @event.ConduitInfo.Shape;
            singleConduitInfo.ColorMarking = @event.ConduitInfo.ColorMarking;
            singleConduitInfo.TextMarking = @event.ConduitInfo.TextMarking;
            singleConduitInfo.Name = @event.ConduitInfo.Name;
            singleConduitInfo.InnerDiameter = @event.ConduitInfo.InnerDiameter;
            singleConduitInfo.OuterDiameter = @event.ConduitInfo.OuterDiameter;

            // Create segment info (as is looks before any cuts or connections)
            var segment = new SingleConduitSegmentInfo();
            segment.Id = Guid.NewGuid();
            segment.ConduitId = @event.SingleConduitId;
            segment.SequenceNumber = 1;
            segment.FromRouteNodeId = routeNetworkQueryService.GetWalkOfInterestInfo(singleConduitInfo.WalkOfInterestId).StartNodeId;
            segment.ToRouteNodeId = routeNetworkQueryService.GetWalkOfInterestInfo(singleConduitInfo.WalkOfInterestId).EndNodeId;

            singleConduitInfo.Segments = new List<ILineSegment>() { segment };

            conduitNetworkQueryService.UpdateSingleConduitInfo(singleConduitInfo);
        }

        private void SingleConduitConnected(SingleConduitInfo singleConduitInfo, SingleConduitConnected @event)
        {
            // Get the multi conduit
            var singleConduit = conduitNetworkQueryService.GetSingleConduitInfo(@event.SingleConduitId);

            // Get the walk of interest of the multi conduit
            var multiConduitWalkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(singleConduit.WalkOfInterestId);

            // Get the node
            var nodeWhereToConnect = routeNetworkQueryService.GetRouteNodeInfo(@event.PointOfInterestId);

            if (@event.ConnectedEndKind == ConduitEndKindEnum.Incomming)
            {
                var segmentToConnect = singleConduitInfo.Segments.Find(s => s.ToRouteNodeId == @event.PointOfInterestId);
                segmentToConnect.ToNodeId = @event.ConnectedJunctionId;
            }
            else
            {
                var segmentToConnect = singleConduitInfo.Segments.Find(s => s.FromRouteNodeId == @event.PointOfInterestId);
                segmentToConnect.FromNodeId = @event.ConnectedJunctionId;
            }

            singleConduitInfo.Name = singleConduitInfo.Name;
            conduitNetworkQueryService.UpdateSingleConduitInfo(singleConduitInfo);
        }

        private void InnerConduitAdded(SingleConduitInfo singleConduitInfo, MultiConduitInnerConduitAdded @event)
        {
            singleConduitInfo.MultiConduitId = @event.MultiConduitId;
            singleConduitInfo.Kind = ConduitKindEnum.InnerConduit;
            singleConduitInfo.SequenceNumber = @event.MultiConduitIndex;
            singleConduitInfo.Id = @event.ConduitInfo.Id;
            singleConduitInfo.Color = @event.ConduitInfo.Color;
            singleConduitInfo.Shape = @event.ConduitInfo.Shape;
            singleConduitInfo.ColorMarking = @event.ConduitInfo.ColorMarking;
            singleConduitInfo.TextMarking = @event.ConduitInfo.TextMarking;
            singleConduitInfo.Name = @event.ConduitInfo.Name;
            singleConduitInfo.InnerDiameter = @event.ConduitInfo.InnerDiameter;
            singleConduitInfo.OuterDiameter = @event.ConduitInfo.OuterDiameter;

            // Create segment info (as is looks before any cuts or connections)
            var multiConduitInfo = conduitNetworkQueryService.GetMultiConduitInfo(@event.MultiConduitId);

            var segment = new SingleConduitSegmentInfo();
            segment.Id = Guid.NewGuid();
            segment.SequenceNumber = 1;
            segment.ConduitId = @event.ConduitInfo.Id;
            segment.FromRouteNodeId = routeNetworkQueryService.GetWalkOfInterestInfo(multiConduitInfo.WalkOfInterestId).StartNodeId;
            segment.ToRouteNodeId = routeNetworkQueryService.GetWalkOfInterestInfo(multiConduitInfo.WalkOfInterestId).EndNodeId;

            singleConduitInfo.Segments = new List<ILineSegment>() { segment };


            conduitNetworkQueryService.UpdateSingleConduitInfo(singleConduitInfo);
        }

        private void InnerConduitCut(SingleConduitInfo singleConduitInfo, MultiConduitInnerConduitCut @event)
        {
            // Get the multi conduit
            var multiConduit = conduitNetworkQueryService.GetMultiConduitInfo(@event.MultiConduitId);

            // Get the walk of interest of the multi conduit
            var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(multiConduit.WalkOfInterestId);

            // Get the node
            var nodeWhereToCut = routeNetworkQueryService.GetRouteNodeInfo(@event.PointOfInterestId);

            ConduitCutter.CutConduit(singleConduitInfo, walkOfInterest, nodeWhereToCut);

            conduitNetworkQueryService.UpdateSingleConduitInfo(singleConduitInfo);
        }

        private void InnerConduitConnected(SingleConduitInfo singleConduitInfo, MultiConduitInnerConduitConnected @event)
        {
            // Get the multi conduit
            var multiConduit = conduitNetworkQueryService.GetMultiConduitInfo(@event.MultiConduitId);

            // Get the walk of interest of the multi conduit
            var multiConduitWalkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(multiConduit.WalkOfInterestId);

            // Get the node
            var nodeWhereToConnect = routeNetworkQueryService.GetRouteNodeInfo(@event.PointOfInterestId);

            if (@event.ConnectedEndKind == ConduitEndKindEnum.Incomming)
            {
                var segmentToConnect = singleConduitInfo.Segments.Find(s => s.ToRouteNodeId == @event.PointOfInterestId);
                segmentToConnect.ToNodeId = @event.ConnectedJunctionId;
            }
            else
            {
                var segmentToConnect = singleConduitInfo.Segments.Find(s => s.FromRouteNodeId == @event.PointOfInterestId);
                segmentToConnect.FromNodeId = @event.ConnectedJunctionId;
            }

            singleConduitInfo.Name = singleConduitInfo.Name;
            conduitNetworkQueryService.UpdateSingleConduitInfo(singleConduitInfo);
        }
    }
}
