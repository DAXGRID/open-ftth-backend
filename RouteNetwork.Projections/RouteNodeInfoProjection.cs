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
        private RouteNetworkQueryService routeNetworkQueryService = null;

        public RouteNodeInfoProjection(IRouteNetworkQueryService routeNetworkQueryService)
        {
            this.routeNetworkQueryService = (RouteNetworkQueryService)routeNetworkQueryService;

            ProjectEvent<RouteNodeAdded>(OnRouteNodeAdded);
        }

     

        // Update our route node info object
        private void OnRouteNodeAdded(RouteNodeInfo routeNode, RouteNodeAdded @event)
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
