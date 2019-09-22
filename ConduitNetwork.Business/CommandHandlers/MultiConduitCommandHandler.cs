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
    public class MultiConduitCommandHandler : 
        IRequestHandler<PlaceMultiConduitCommand>,
        IRequestHandler<CutOuterConduitCommand>,
        IRequestHandler<CutInnerConduitCommand>,
        IRequestHandler<ConnectInnerConduitToJunction>
    {
        private readonly IAggregateRepository repo = null;
        private readonly IConduitNetworkQueryService conduitNetworkQueryService = null;
        private readonly IRouteNetworkQueryService routeNetworkQueryService = null;
        private readonly IConduitSpecificationRepository conduitSpecificationRepository = null;

        public MultiConduitCommandHandler(IAggregateRepository aggregateRepository, IRouteNetworkQueryService routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IConduitSpecificationRepository conduitSpecificationRepository)
        {
            this.repo = aggregateRepository;
            this.conduitNetworkQueryService = conduitNetworkQueryService;
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitSpecificationRepository = conduitSpecificationRepository;
        }

        public Task<Unit> Handle(PlaceMultiConduitCommand request, CancellationToken cancellationToken)
        {
            var multiConduit = new MultiConduit(request.MultiConduitId, request.WalkOfInterestId, request.ConduitSpecificationId, request.Name, request.MarkingColor, request.MarkingText, conduitNetworkQueryService, conduitSpecificationRepository, request.DemoDataSpec);
            repo.Store(multiConduit);

            return Unit.Task;
        }

        public Task<Unit> Handle(CutInnerConduitCommand request, CancellationToken cancellationToken)
        {
            // Id check
            if (request.MultiConduitId == null || request.MultiConduitId == Guid.Empty)
                throw new ArgumentException("MultiConduitId cannot be null or empty");

            var multiConduit = repo.Load<MultiConduit>(request.MultiConduitId);
                                    
            multiConduit.CutInnerConduit(request.InnerConduitSequenceNumber, request.PointOfInterestId, routeNetworkQueryService, conduitNetworkQueryService);
            repo.Store(multiConduit);

            return Unit.Task;
        }

        public Task<Unit> Handle(ConnectInnerConduitToJunction request, CancellationToken cancellationToken)
        {
            // Id check
            if (request.MultiConduitId == null || request.MultiConduitId == Guid.Empty)
                throw new ArgumentException("MultiConduitId cannot be null or empty");

            var multiConduit = repo.Load<MultiConduit>(request.MultiConduitId);


            multiConduit.ConnectInnerConduit(request.PointOfInterestId, request.InnerConduitSequenceNumber, request.ConnectedEndKind, request.ConnectedJunctionId, routeNetworkQueryService, conduitNetworkQueryService);
            repo.Store(multiConduit);

            return Unit.Task;
        }

        public Task<Unit> Handle(CutOuterConduitCommand request, CancellationToken cancellationToken)
        {
            // Id check
            if (request.MultiConduitId == null || request.MultiConduitId == Guid.Empty)
                throw new ArgumentException("MultiConduitId cannot be null or empty");

            var multiConduit = repo.Load<MultiConduit>(request.MultiConduitId);

            multiConduit.CutOuterConduit(request.PointOfInterestId, routeNetworkQueryService, conduitNetworkQueryService);
            repo.Store(multiConduit);

            return Unit.Task;
        }
    }
}
