using ConduitNetwork.QueryService;
using FiberNetwork.Business.Aggregates;
using FiberNetwork.Business.Commands;
using FiberNetwork.QueryService;
using Infrastructure.EventSourcing;
using MediatR;
using RouteNetwork.QueryService;
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
        private readonly IFiberNetworkQueryService fiberNetworkQueryService = null;

        public FiberCableCommandHandler(IAggregateRepository aggregateRepository, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IFiberNetworkQueryService fiberNetworkQueryService)
        {
            this.repo = aggregateRepository;
            this.conduitNetworkQueryService = conduitNetworkQueryService;
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.fiberNetworkQueryService = fiberNetworkQueryService;
        }

        public Task<Unit> Handle(PlaceFiberCableCommand request, CancellationToken cancellationToken)
        {
            var fiberCable = new FiberCable(request.FiberCableId, request.WalkOfInterestId, request.NumberOfFibers, request.Name, fiberNetworkQueryService);
            repo.Store(fiberCable);

            return Unit.Task;
        }
    }
}
