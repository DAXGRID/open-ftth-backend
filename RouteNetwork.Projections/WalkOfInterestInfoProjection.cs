using Infrastructure.EventSourcing;
using Marten.Events.Projections;
using RouteNetwork.Events;
using RouteNetwork.Events.Model;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Projections
{
    
    public sealed class WalkOfInterestInfoProjection : ViewProjection<WalkOfInterestInfo, Guid>
    {
        private RouteNetworkQueryService routeNetworkQueryService = null;

        public WalkOfInterestInfoProjection(IRouteNetworkQueryService routeNetworkQueryService)
        {
            this.routeNetworkQueryService = (RouteNetworkQueryService)routeNetworkQueryService;

            ProjectEvent<WalkOfInterestRegistered>(OnRouteSegmentAdded);
        }
     

        // Update our route node info object
        private void OnRouteSegmentAdded(WalkOfInterestInfo walkOfInterestInfo, WalkOfInterestRegistered @event)
        {
            walkOfInterestInfo.RouteElementIds = @event.RouteElementIds;

            routeNetworkQueryService.AddWalkOfInterestInfo(walkOfInterestInfo);
        }
    }
}
