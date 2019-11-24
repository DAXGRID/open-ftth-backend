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

namespace ConduitNetwork.Business.CommandHandlers
{
    public class MultiConduitCommandHandler : 
        IRequestHandler<PlaceMultiConduitCommand>,
        IRequestHandler<CutOuterConduitCommand>,
        IRequestHandler<CutInnerConduitCommand>,
        IRequestHandler<ConnectInnerConduitToJunction>,
        IRequestHandler<AddInnerConduitCommand>,
        IRequestHandler<ConnectConduitSegmentCommand>
    {
        private readonly IAggregateRepository repo = null;
        private readonly IConduitNetworkQueryService conduitNetworkQueryService = null;
        private readonly IRouteNetworkState routeNetworkQueryService = null;
        private readonly IConduitSpecificationRepository conduitSpecificationRepository = null;

        public MultiConduitCommandHandler(IAggregateRepository aggregateRepository, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IConduitSpecificationRepository conduitSpecificationRepository)
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
            if (request.InnerConduitId == null && request.MultiConduitId == null)
                throw new ArgumentException("Either InnerConduitId or MultiConduitId + InnerConduitSequenceNumber must be provided.");


            if (request.MultiConduitId == null || request.MultiConduitId == Guid.Empty)
            {
                var conduitRelations = conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(request.PointOfInterestId);

                if (!conduitRelations.Exists(c => c.Segment.ConduitId == request.InnerConduitId))
                    throw new ArgumentException("Cannot find inner conduit with id: " + request.InnerConduitId + " in point of interest: " + request.PointOfInterestId);

                var innerConduitSegment = conduitRelations.Find(c => c.Segment.ConduitId == request.InnerConduitId);

                request.MultiConduitId = innerConduitSegment.Segment.Conduit.GetRootConduit().Id;
                request.InnerConduitSequenceNumber = innerConduitSegment.Segment.Conduit.SequenceNumber;
            }


            var multiConduit = repo.Load<MultiConduit>(request.MultiConduitId);
                                    
            multiConduit.CutInnerConduit(request.InnerConduitSequenceNumber, request.PointOfInterestId, routeNetworkQueryService, conduitNetworkQueryService);
            repo.Store(multiConduit);

            return Unit.Task;
        }

        public Task<Unit> Handle(AddInnerConduitCommand request, CancellationToken cancellationToken)
        {
            // Id check
            if (request.MultiConduitId == null || request.MultiConduitId == Guid.Empty)
                throw new ArgumentException("MultiConduitId cannot be null or empty");

            var multiConduit = repo.Load<MultiConduit>(request.MultiConduitId);

            multiConduit.AddInnerConduit(request.Color, request.OuterDiameter, request.InnerDiameter, routeNetworkQueryService, conduitNetworkQueryService);
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

        public Task<Unit> Handle(ConnectConduitSegmentCommand request, CancellationToken cancellationToken)
        {
            var conduitRelations = conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(request.PointOfInterestId);

            if (!conduitRelations.Exists(c => c.Segment.Id == request.FromConduitSegmentId))
                throw new ArgumentException("Cannot find from conduit segment: " + request.FromConduitSegmentId + " in point of interest: " + request.PointOfInterestId);

            if (!conduitRelations.Exists(c => c.Segment.Id == request.ToConduitSegmentId))
                throw new ArgumentException("Cannot find to conduit segment: " + request.ToConduitSegmentId + " in point of interest: " + request.PointOfInterestId);

            var fromConduitSegmentRel = conduitRelations.Find(c => c.Segment.Id == request.FromConduitSegmentId);
            var toConduitSegmentRel = conduitRelations.Find(c => c.Segment.Id == request.ToConduitSegmentId);

            // Junction id
            Guid junctionId = Guid.NewGuid();

            // Find from direction
            ConduitEndKindEnum fromEndKind = ConduitEndKindEnum.Incomming;

            if (fromConduitSegmentRel.Type == ReadModel.ConduitRelationTypeEnum.Incomming)
                fromEndKind = ConduitEndKindEnum.Incomming;
            else if (fromConduitSegmentRel.Type == ReadModel.ConduitRelationTypeEnum.Outgoing)
                fromEndKind = ConduitEndKindEnum.Outgoing;
            else
               throw new ArgumentException("From conduit segment: " + request.FromConduitSegmentId + " is " + fromConduitSegmentRel.Type.ToString() + ". Must be incomming or outgoing (cut in node) to be connected");

            // Find to direction
            ConduitEndKindEnum toEndKind = ConduitEndKindEnum.Incomming;

            if (toConduitSegmentRel.Type == ReadModel.ConduitRelationTypeEnum.Incomming)
                toEndKind = ConduitEndKindEnum.Incomming;
            else if (toConduitSegmentRel.Type == ReadModel.ConduitRelationTypeEnum.Outgoing)
                toEndKind = ConduitEndKindEnum.Outgoing;
            else
                throw new ArgumentException("To conduit segment: " + request.ToConduitSegmentId + " is " + toConduitSegmentRel.Type.ToString() + ". Must be incomming or outgoing (cut in node) to be connected");


            // If connection between inner conduit and multi conduit (flex situation)
            if (fromConduitSegmentRel.Segment.Conduit.Kind == ConduitKindEnum.InnerConduit && toConduitSegmentRel.Segment.Conduit.Kind == ConduitKindEnum.MultiConduit)
            {
                // Connect to inner conduit to junction
                var fromMultiConduit = repo.Load<MultiConduit>(fromConduitSegmentRel.Segment.Conduit.Parent.Id);
                fromMultiConduit.ConnectInnerConduit(request.PointOfInterestId, fromConduitSegmentRel.Segment.Conduit.SequenceNumber, fromEndKind, junctionId, routeNetworkQueryService, conduitNetworkQueryService);
                repo.Store(fromMultiConduit);

                // Create inner conduit in multi conduit
                var toMultiConduit = repo.Load<MultiConduit>(toConduitSegmentRel.Segment.Conduit.Id);

                var toInnerConduitSeqNo = toMultiConduit.AddInnerConduit(fromConduitSegmentRel.Segment.Conduit.Color, fromConduitSegmentRel.Segment.Conduit.OuterDiameter, fromConduitSegmentRel.Segment.Conduit.InnerDiameter, routeNetworkQueryService, conduitNetworkQueryService);
                repo.Store(toMultiConduit);

                // Connect new inner conduit to junction
                toMultiConduit.ConnectInnerConduit(request.PointOfInterestId, toInnerConduitSeqNo, toEndKind, junctionId, routeNetworkQueryService, conduitNetworkQueryService);
                repo.Store(toMultiConduit);
            }
            // If connection between inner conduit and inner conduit
            else if (fromConduitSegmentRel.Segment.Conduit.Kind == ConduitKindEnum.InnerConduit && toConduitSegmentRel.Segment.Conduit.Kind == ConduitKindEnum.InnerConduit)
            {
                // Connect from inner conduit to junction
                var fromMultiConduit = repo.Load<MultiConduit>(fromConduitSegmentRel.Segment.Conduit.Parent.Id);
                fromMultiConduit.ConnectInnerConduit(request.PointOfInterestId, fromConduitSegmentRel.Segment.Conduit.SequenceNumber, fromEndKind, junctionId, routeNetworkQueryService, conduitNetworkQueryService);
                repo.Store(fromMultiConduit);

                // Connect from inner conduit to junction
                var toMultiConduit = repo.Load<MultiConduit>(toConduitSegmentRel.Segment.Conduit.Parent.Id);
                toMultiConduit.ConnectInnerConduit(request.PointOfInterestId, toConduitSegmentRel.Segment.Conduit.SequenceNumber, toEndKind, junctionId, routeNetworkQueryService, conduitNetworkQueryService);
                repo.Store(toMultiConduit);
            }
            else
            {
                throw new NotSupportedException("This kind of connection is not supported. Please check code where from this message is thrown");
            }

            return Unit.Task;
        }
    }
}
