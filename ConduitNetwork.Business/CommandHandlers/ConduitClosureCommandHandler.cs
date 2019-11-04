using ConduitNetwork.Business.Aggregates;
using ConduitNetwork.Business.Commands;
using ConduitNetwork.Events;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using Infrastructure.EventSourcing;
using MediatR;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConduitNetwork.Business.CommandHandlers
{
    public class ConduitClosureCommandHandler :
        IRequestHandler<PlaceConduitClosureCommand>,
        IRequestHandler<RemoveConduitClosureCommand>,
        IRequestHandler<AttachPassByConduitToClosureCommand>,
        IRequestHandler<AttachConduitEndToClosureCommand>
    {
        private readonly IAggregateRepository repo = null;
        private readonly IConduitNetworkQueryService conduitNetworkQueryService = null;
        private readonly IRouteNetworkState routeNetworkQueryService = null;
        private readonly IConduitClosureRepository conduitClosureRepository = null;

        public ConduitClosureCommandHandler(IAggregateRepository aggregateRepository, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IConduitClosureRepository conduitClosureRepository)
        {
            this.repo = aggregateRepository;
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkQueryService = conduitNetworkQueryService;
            this.conduitClosureRepository = conduitClosureRepository;
        }

        public Task<Unit> Handle(PlaceConduitClosureCommand request, CancellationToken cancellationToken)
        {
            // Check if aggregate id has been used
            if (repo.CheckIfAggregateIdHasBeenUsed(request.ConduitClosureId))
                throw new ArgumentException("The uuid: " + request.ConduitClosureId + " has allready been used. This is an event sourced system, so you're not allowed to reuse object uuids.");

            var conduitClosure = new ConduitClosure(request.ConduitClosureId, request.PointOfInterestId, routeNetworkQueryService, conduitNetworkQueryService, conduitClosureRepository);

            repo.Store(conduitClosure);

            return Unit.Task;
        }

        public Task<Unit> Handle(RemoveConduitClosureCommand request, CancellationToken cancellationToken)
        {
            // Id check
            if (request.ConduitClosureId == null || request.ConduitClosureId == Guid.Empty)
                throw new ArgumentException("ConduitClosureId cannot be null or empty");

            var conduitClosure = repo.Load<ConduitClosure>(request.ConduitClosureId);
            conduitClosure.Remove();

            repo.Store(conduitClosure);

            return Unit.Task;
        }

        public Task<Unit> Handle(AttachPassByConduitToClosureCommand request, CancellationToken cancellationToken)
        {
            // Id check
            if (request.ConduitClosureId == null || request.ConduitClosureId == Guid.Empty)
                throw new ArgumentException("ConduitClosureId cannot be null or empty");

            var conduitClosure = repo.Load<ConduitClosure>(request.ConduitClosureId);

            conduitClosure.AttachPassByConduitToClosure(request.ConduitId, request.IncommingSide, request.OutgoingSide, request.IncommingPortPosition, request.OutgoingPortPosition, request.IncommingTerminalPosition, request.OutgoingTerminalPosition, routeNetworkQueryService, conduitNetworkQueryService, conduitClosureRepository);
            repo.Store(conduitClosure);

            return Unit.Task;
        }

        public Task<Unit> Handle(AttachConduitEndToClosureCommand request, CancellationToken cancellationToken)
        {
            // Id check
            if (request.ConduitClosureId == null || request.ConduitClosureId == Guid.Empty)
                throw new ArgumentException("ConduitClosureId cannot be null or empty");

            var conduitClosure = repo.Load<ConduitClosure>(request.ConduitClosureId);

            conduitClosure.AttachConduitEndToClosure(request.ConduitId, request.Side, request.PortPosition, request.TerminalPosition, routeNetworkQueryService, conduitNetworkQueryService, conduitClosureRepository);
            repo.Store(conduitClosure);

            return Unit.Task;
        }

    }
}
