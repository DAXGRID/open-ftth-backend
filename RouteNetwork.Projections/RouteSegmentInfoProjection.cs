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
    
    public sealed class RouteSegmentInfoProjection : ViewProjection<RouteSegmentInfo, Guid>
    {
        private RouteNetworkState routeNetworkQueryService = null;

        public RouteSegmentInfoProjection(IRouteNetworkState routeNetworkQueryService)
        {
            this.routeNetworkQueryService = (RouteNetworkState)routeNetworkQueryService;

            ProjectEvent<RouteSegmentPlanned>(
               (session, domainEvent) =>
               {
                   return domainEvent.Id;
               },
               OnRouteSegmentAdded
           );


            //ProjectEvent<RouteSegmentPlanned>(OnRouteSegmentAdded);
        }
     

        // Update our route node info object
        private void OnRouteSegmentAdded(RouteSegmentInfo routeSegment, RouteSegmentPlanned @event)
        {
            routeSegment.FromNodeId = @event.FromNodeId;
            routeSegment.ToNodeId = @event.ToNodeId;
            routeSegment.SegmentKind = @event.SegmentKind;
            routeSegment.Geometry = @event.InitialGeometry;

            routeNetworkQueryService.AddRouteSegmentInfo(routeSegment);
        }
    }
}
