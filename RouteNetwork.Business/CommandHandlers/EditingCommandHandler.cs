using Infrastructure.EventSourcing;
using MediatR;
using RouteNetwork.Business.Aggregates;
using RouteNetwork.Business.Commands;
using RouteNetwork.Projections;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RouteNetwork.Business.CommandHandlers
{
    public class EditingCommandHandler : 
        IRequestHandler<AddNodeCommand>, 
        IRequestHandler<AddSegmentCommand>,
        IRequestHandler<SplitSegmentCommand>
    {
        private readonly IAggregateRepository repo = null;
        private readonly IRouteNetworkState routeQueryService = null;
        private readonly RouteNetworkAggregate routeNetworkAggregate = null;

        public EditingCommandHandler(IAggregateRepository aggregateRepository, RouteNetworkAggregate routeNetworkAggregate, IRouteNetworkState routeQueryService)
        {
            this.repo = aggregateRepository;
            this.routeQueryService = routeQueryService;
            this.routeNetworkAggregate = routeNetworkAggregate;
        }

        public Task<Unit> Handle(AddNodeCommand request, CancellationToken cancellationToken)
        {
            routeNetworkAggregate.AddRouteNode(request.Id, request.Name, request.NodeKind, request.NodeFunctionKind, request.Geometry, request.LocationInfo);
            repo.Store(routeNetworkAggregate);

            /*
            // Id check
            if (request.Id == null || request.Id == Guid.Empty)
                throw new ArgumentException("Id cannot be null or empty");

            // Check that not already exists
            if (routeQueryService.CheckIfRouteNodeIdExists(request.Id))
            {
                throw new ArgumentException("A route node with id: " + request.Id + " already exists");
            }

            var routeNode = new RouteNode(request.Id, request.Name, request.NodeKind, request.NodeFunctionKind, request.Geometry, request.LocationInfo);
            

            repo.Store(routeNode);
            */

            return Unit.Task;
        }

        public Task<Unit> Handle(AddSegmentCommand request, CancellationToken cancellationToken)
        {
            routeNetworkAggregate.AddRouteSegment(request.Id, request.FromNodeId, request.ToNodeId, request.SegmentKind, request.Geometry);
            repo.Store(routeNetworkAggregate);

            /*

            // Id check
            if (request.Id == null || request.Id == Guid.Empty)
                throw new ArgumentException("Id cannot be null or empty");

            // Check that not already exists
            if (routeQueryService.CheckIfRouteSegmentIdExists(request.Id))
            {
                throw new ArgumentException("A route segment with id: " + request.Id + " already exists");
            }

            if (!routeQueryService.CheckIfRouteNodeIdExists(request.FromNodeId))
            {
                throw new ArgumentException("Cannot find from node with id: " + request.FromNodeId);
            }

            if (!routeQueryService.CheckIfRouteNodeIdExists(request.ToNodeId))
            {
                throw new ArgumentException("Cannot find to node with id: " + request.FromNodeId);
            }

            var routeSegment = new RouteSegment(request.Id,  request.FromNodeId, request.ToNodeId, request.SegmentKind, request.Geometry);
            repo.Store(routeSegment);
            */

            return Unit.Task;
        }

        public Task<Unit> Handle(SplitSegmentCommand request, CancellationToken cancellationToken)
        {
            routeNetworkAggregate.SplitRouteSegment(request.SegmentId, request.NodeId);
            repo.Store(routeNetworkAggregate);

            /*
            // Id check
            if (request.SegmentId == null || request.SegmentId == Guid.Empty)
                throw new ArgumentException("Segment id cannot be null or empty");
         
            var segment = repo.Load<RouteSegment>(request.SegmentId);

            segment.Split(request.NodeId, routeQueryService);
            repo.Store(segment);
            */

            return Unit.Task;
        }


    }
}
