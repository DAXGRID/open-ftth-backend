using Core.ReadModel.Network;
using FiberNetwork.Events;
using FiberNetwork.Events.Model;
using FiberNetwork.QueryService;
using Marten.Events.Projections;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;

namespace FiberNetwork.Projections
{
    public sealed class FiberCableInfoProjection : ViewProjection<FiberCableInfo, Guid>
    {
        private IRouteNetworkState routeNetworkQueryService = null;
        private FiberNetworkQueryService fiberNetworkQueryService = null;

        public FiberCableInfoProjection(IRouteNetworkState routeNetworkQueryService, IFiberNetworkQueryService fiberNetworkQueryService)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.fiberNetworkQueryService = (FiberNetworkQueryService)fiberNetworkQueryService;

            ProjectEvent<FiberCablePlaced>(OnFiberCablePlaced);
        }

        private void OnFiberCablePlaced(FiberCableInfo fiberCableInfo, FiberCablePlaced @event)
        {
            fiberCableInfo.Id = @event.FiberCableId;
            fiberCableInfo.WalkOfInterestId = @event.WalkOfInterestId;

            // Create segment info (as is looks before any cuts or connections)
            var segment = new FiberCableSegmentInfo();
            segment.Id = Guid.NewGuid();
            segment.FiberCableId = @event.FiberCableId;
            segment.SequenceNumber = 1;
            segment.FromRouteNodeId = routeNetworkQueryService.GetWalkOfInterestInfo(fiberCableInfo.WalkOfInterestId).StartNodeId;
            segment.ToRouteNodeId = routeNetworkQueryService.GetWalkOfInterestInfo(fiberCableInfo.WalkOfInterestId).EndNodeId;

            fiberCableInfo.Segments = new List<ILineSegment>() { segment };

            // Create all the children
            fiberCableInfo.Children = new List<ILine>();

            fiberNetworkQueryService.UpdateMultiConduitInfo(fiberCableInfo);
        }
    }
}
