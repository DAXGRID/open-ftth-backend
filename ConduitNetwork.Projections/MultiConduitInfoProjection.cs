using ConduitNetwork.Events;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.ReadModel;
using Core.ReadModel.Network;
using Marten.Events.Projections;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;

namespace ConduitNetwork.Projections
{
    public sealed class MultiConduitInfoProjection : ViewProjection<MultiConduitInfo, Guid>
    {
        private IRouteNetworkState routeNetworkQueryService = null;
        private ConduitNetworkQueryService conduitNetworkQueryService = null;

        public MultiConduitInfoProjection(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkQueryService = (ConduitNetworkQueryService)conduitNetworkQueryService;

            ProjectEvent<MultiConduitPlaced>(OnMultiConduitPlaced);
            ProjectEvent<MultiConduitOuterConduitCut>(OuterConduitCut);
            

        }

        private void OnMultiConduitPlaced(MultiConduitInfo multiConduitInfo, MultiConduitPlaced @event)
        {
            multiConduitInfo.Id = @event.MultiConduitId;
            multiConduitInfo.Kind = ConduitKindEnum.MultiConduit;
            multiConduitInfo.WalkOfInterestId = @event.WalkOfInterestId;
            multiConduitInfo.AssetInfo = @event.AssetInfo;
            multiConduitInfo.Color = @event.ConduitInfo.Color;
            multiConduitInfo.Shape = @event.ConduitInfo.Shape;
            multiConduitInfo.ColorMarking = @event.ConduitInfo.ColorMarking;
            multiConduitInfo.TextMarking = @event.ConduitInfo.TextMarking;
            multiConduitInfo.Name = @event.ConduitInfo.Name;
            multiConduitInfo.InnerDiameter = @event.ConduitInfo.InnerDiameter;
            multiConduitInfo.OuterDiameter = @event.ConduitInfo.OuterDiameter;

            multiConduitInfo.Children = new List<ILine>();

            // Create segment info (as is looks before any cuts or connections)
            var segment = new MultiConduitSegmentInfo();
            segment.Id = Guid.NewGuid();
            segment.ConduitId = @event.MultiConduitId;
            segment.SequenceNumber = 1;
            segment.FromRouteNodeId = routeNetworkQueryService.GetWalkOfInterestInfo(multiConduitInfo.WalkOfInterestId).StartNodeId;
            segment.ToRouteNodeId = routeNetworkQueryService.GetWalkOfInterestInfo(multiConduitInfo.WalkOfInterestId).EndNodeId;

            multiConduitInfo.Segments = new List<ISegment>() { segment };

            conduitNetworkQueryService.UpdateMultiConduitInfo(multiConduitInfo);
        }

        private void OuterConduitCut(MultiConduitInfo multiConduitInfo, MultiConduitOuterConduitCut @event)
        {
            // Get the multi conduit
            var multiConduit = conduitNetworkQueryService.GetMultiConduitInfo(@event.MultiConduitId);

            // Get the walk of interest of the multi conduit
            var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(multiConduit.WalkOfInterestId);

            // Get the node
            var nodeWhereToCut = routeNetworkQueryService.GetRouteNodeInfo(@event.PointOfInterestId);

            ConduitCutter.CutConduit(multiConduitInfo, walkOfInterest, nodeWhereToCut);

            conduitNetworkQueryService.UpdateMultiConduitInfo(multiConduitInfo);
        }
    }
}
