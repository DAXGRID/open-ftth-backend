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
    
    public sealed class RouteNodeInfoProjection : ViewProjection<RouteNodeInfo, Guid>
    {
        private RouteNetworkState routeNetworkQueryService = null;

        public RouteNodeInfoProjection(IRouteNetworkState routeNetworkQueryService)
        {
            this.routeNetworkQueryService = (RouteNetworkState)routeNetworkQueryService;

            ProjectEvent<RouteNodePlanned>(
                (session, domainEvent) =>
                {
                    return domainEvent.Id;
                },
                OnRouteNodeAdded
            );
        }

        // Update our route node info object
        private void OnRouteNodeAdded(RouteNodeInfo routeNode, RouteNodePlanned @event)
        {
            routeNode.Name = @event.Name;
            routeNode.NodeKind = @event.NodeKind;
            routeNode.NodeFunctionKind = @event.NodeFunctionKind;
            routeNode.Geometry = @event.InitialGeometry;
            routeNode.LocationInfo = @event.LocationInfo;

            routeNetworkQueryService.AddRouteNodeInfo(routeNode);
        }
    }
}
