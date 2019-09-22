using ConduitNetwork.Events;
using ConduitNetwork.QueryService;
using ConduitNetwork.ReadModel;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Projections
{
    public class ConduitCutHandler
    {
        private IRouteNetworkQueryService routeNetworkQueryService = null;
        private ConduitNetworkQueryService conduitNetworkQueryService = null;

        public ConduitCutHandler(IRouteNetworkQueryService routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkQueryService = (ConduitNetworkQueryService)conduitNetworkQueryService;
        }

        public void InnerConduitCut(SingleConduitInfo singleConduitInfo, MultiConduitInnerConduitCut @event)
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
    }
}
