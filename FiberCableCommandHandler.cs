using ConduitNetwork.Business.Aggregates;
using ConduitNetwork.Business.Commands;
using ConduitNetwork.Business.Specifications;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using Infrastructure.EventSourcing;
using MediatR;
using RouteNetwork.QueryService;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FiberNetwork.Business.CommandHandlers
{
    public class FiberCableCommandHandler :
        IRequestHandler<PlaceFiberCableCommand>
    {
        private readonly IAggregateRepository repo = null;
        private readonly IConduitNetworkQueryService conduitNetworkQueryService = null;
        private readonly IRouteNetworkState routeNetworkQueryService = null;
        private readonly IConduitSpecificationRepository conduitSpecificationRepository = null;

        public FiberCableCommandHandler(IAggregateRepository aggregateRepository, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IConduitSpecificationRepository conduitSpecificationRepository)
        {
            this.repo = aggregateRepository;
            this.conduitNetworkQueryService = conduitNetworkQueryService;
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitSpecificationRepository = conduitSpecificationRepository;
        }

        public Task<Unit> Handle(PlaceFiberCableCommand request, CancellationToken cancellationToken)
        {
            /*
            var multiConduit = new MultiConduit(request.MultiConduitId, request.WalkOfInterestId, request.ConduitSpecificationId, request.Name, request.MarkingColor, request.MarkingText, conduitNetworkQueryService, conduitSpecificationRepository, request.DemoDataSpec);
            repo.Store(multiConduit);
            */

            return Unit.Task;
        }
    }
}
