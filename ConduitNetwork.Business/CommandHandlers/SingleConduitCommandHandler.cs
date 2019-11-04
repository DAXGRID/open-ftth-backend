using ConduitNetwork.Business.Aggregates;
using ConduitNetwork.Business.Commands;
using ConduitNetwork.Business.Specifications;
using ConduitNetwork.Events;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
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
    public class SingleConduitCommandHandler :
        IRequestHandler<PlaceSingleConduitCommand>, 
        IRequestHandler<CutSingleConduitCommand>,
        IRequestHandler<ConnectSingleConduitToJunction>
    {
        private readonly IAggregateRepository repo = null;
        private readonly IConduitNetworkQueryService conduitNetworkQueryService = null;
        private readonly IRouteNetworkState routeNetworkQueryService = null;
        private readonly IConduitSpecificationRepository conduitSpecificationRepository = null;


        public SingleConduitCommandHandler(IAggregateRepository aggregateRepository, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IConduitSpecificationRepository conduitSpecificationRepository)
        {
            this.repo = aggregateRepository;
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkQueryService = conduitNetworkQueryService;
            this.conduitSpecificationRepository = conduitSpecificationRepository;
        }

        public Task<Unit> Handle(PlaceSingleConduitCommand request, CancellationToken cancellationToken)
        {
            var singleConduit = new SingleConduit(request.SingleConduitId, request.WalkOfInterestId, request.ConduitSpecificationId, request.Name, request.MarkingColor, request.MarkingText, conduitNetworkQueryService, conduitSpecificationRepository, request.DemoDataSpec);
            repo.Store(singleConduit);

            return Unit.Task;
        }

        public Task<Unit> Handle(CutSingleConduitCommand request, CancellationToken cancellationToken)
        {
            // Id check
            if (request.SingleConduitId == null || request.SingleConduitId == Guid.Empty)
                throw new ArgumentException("MultiConduitId cannot be null or empty");


            var singleConduit = repo.Load<SingleConduit>(request.SingleConduitId);

            singleConduit.Cut(request.PointOfInterestId, routeNetworkQueryService, conduitNetworkQueryService);
            repo.Store(singleConduit);

            return Unit.Task;
        }

        public Task<Unit> Handle(ConnectSingleConduitToJunction request, CancellationToken cancellationToken)
        {
            // Id check
            if (request.SingleConduitId == null || request.SingleConduitId == Guid.Empty)
                throw new ArgumentException("MultiConduitId cannot be null or empty");
                     

            var singleConduit = repo.Load<SingleConduit>(request.SingleConduitId);

            singleConduit.Connect(request.PointOfInterestId, request.ConnectedEndKind, request.ConnectedJunctionId, routeNetworkQueryService, conduitNetworkQueryService);
            repo.Store(singleConduit);

            return Unit.Task;
        }
    }
}
