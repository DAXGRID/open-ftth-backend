using ConduitNetwork.Events;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel;
using ConduitNetwork.ReadModel.ConduitClosure;
using Marten.Events.Projections;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;

namespace ConduitNetwork.Projections.ConduitClosure
{
    public sealed class ConduitClosureLifecyleEventProjection : ViewProjection<ConduitClosureInfo, Guid>
    {
        private IRouteNetworkQueryService routeNetworkQueryService = null;
        private IConduitNetworkQueryService conduitNetworkQueryService = null;
        private IConduitClosureRepository conduitClosureRepository = null;

        public ConduitClosureLifecyleEventProjection(IRouteNetworkQueryService routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IConduitClosureRepository conduitClosureRepository)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkQueryService = conduitNetworkQueryService;
            this.conduitClosureRepository = conduitClosureRepository;

            // Conduits closure placed
            ProjectEvent<ConduitClosurePlaced>(
                (session, e) =>
                {
                    return e.ConduitClosureId;
                },
                OnConduitClosurePlaced);

            // Conduit closure removed
            DeleteEvent<ConduitClosureRemoved>(
                (session, e) =>
                {
                    return e.ConduitClosureId;
                },
                OnConduitClosureRemoved);
        }

        private void OnConduitClosurePlaced(ConduitClosureInfo conduitClosureInfo, ConduitClosurePlaced @event)
        {
            conduitClosureInfo.Id = @event.ConduitClosureId;
            conduitClosureInfo.PointOfInterestId = @event.PointOfInterestId;

            // Add four sides
            conduitClosureInfo.Sides = new List<ConduitClosureSideInfo>();
            for (int i = 0; i < 4; i++)
                conduitClosureInfo.Sides.Add(new ConduitClosureSideInfo()
                {
                    Position = (ConduitClosureSideEnum)i + 1,
                    Ports = new List<ConduitClosurePortInfo>()
                }); ;

            conduitClosureRepository.UpdateConduitClosureInfo(conduitClosureInfo);
        }

        private bool OnConduitClosureRemoved(ConduitClosureInfo conduitClosureInfo, ConduitClosureRemoved @event)
        {
            conduitClosureRepository.RemoveConduitClosureInfo(@event.ConduitClosureId);

            return true;
        }

    }
}
