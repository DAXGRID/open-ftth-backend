using ConduitNetwork.Events;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.ReadModel;
using Marten.Events.Projections;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;

namespace ConduitNetwork.Projections
{
    public sealed class xSingleConduitCutProjection : ViewProjection<SingleConduitInfo, Guid>
    {
        private IRouteNetworkQueryService routeNetworkQueryService = null;
        private ConduitNetworkQueryService conduitNetworkQueryService = null;

        public xSingleConduitCutProjection(IRouteNetworkQueryService routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkQueryService = (ConduitNetworkQueryService)conduitNetworkQueryService;
            

            // We also want to update the single conduit info when a inner conduit of a multi conduit is cut
            ProjectEvent<MultiConduitInnerConduitCut>(
            (session, innerConduitAdded) =>
            {
                // To get to the right single conduit, we need look it up through the multi conduit
                var mutltiConduitInfo = this.conduitNetworkQueryService.GetMultiConduitInfo(innerConduitAdded.MultiConduitId);
                return mutltiConduitInfo.Children.Find(c => c.Position == innerConduitAdded.InnerConduitSequenceNumber).Id;
            },
            InnerConduitCut);
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

    }
}
