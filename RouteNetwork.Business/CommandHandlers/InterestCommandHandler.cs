using Core.GraphSupport.Model;
using Infrastructure.EventSourcing;
using MediatR;
using RouteNetwork.Business.Aggregates;
using RouteNetwork.Business.Commands;
using RouteNetwork.Projections;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RouteNetwork.Business.CommandHandlers
{
    public class InterestCommandHandler : IRequestHandler<RegisterWalkOfInterestCommand>
    {
        private readonly IAggregateRepository repo = null;
        private readonly IRouteNetworkQueryService routeQueryService = null;

        public InterestCommandHandler(IAggregateRepository aggregateRepository, IRouteNetworkQueryService routeQueryService)
        {
            this.repo = aggregateRepository;
            this.routeQueryService = routeQueryService;
        }

        public Task<Unit> Handle(RegisterWalkOfInterestCommand request, CancellationToken cancellationToken)
        {
            // Id check
            if (request.WalkOfInterestId == null || request.WalkOfInterestId == Guid.Empty)
                throw new ArgumentException("Id cannot be null or empty");

            // Basic check that some route element ids are filled in at all
            if (request.RouteElementIds == null || request.RouteElementIds.Count < 3)
            {
                throw new ArgumentException("The route element id list is empty or has less than 3 ids, which cannot possible be a valid walk. A minumum walk is from one node, through a segment, to another node (node->segment->node) - i.e. 3 ids.");
            }

            // Check if the chain of route element ids are valid (is proper connected to each other in the route network graph)
            bool shouldBeNode = true;
            bool firstElement = true;
            GraphElement previousElement = null;
            Guid previousElementId = Guid.Empty;

            int position = 1;

            foreach (var routeElementId in request.RouteElementIds)
            {
                GraphElement currentElement = null;

                // Node
                if (shouldBeNode)
                {
                    if (!routeQueryService.CheckIfRouteNodeIdExists(routeElementId))
                        throw new ArgumentException("Route element id: " + routeElementId + " at position: " + position + " (in the route element ids property) is expected to be a node. But no node could be found with that id.");

                    currentElement = routeQueryService.GetRouteNodeInfo(routeElementId);

                    shouldBeNode = false;
                }

                // Segment
                else if (!shouldBeNode)
                {
                    if (!routeQueryService.CheckIfRouteSegmentIdExists(routeElementId))
                        throw new ArgumentException("Route element id: " + routeElementId + " at position: " + position + " (in the route element ids property) is expected to be a segment. But no segment could be found with that id.");

                    currentElement = routeQueryService.GetRouteSegmentInfo(routeElementId);

                    shouldBeNode = true;
                }

                // Check that the element is connected to the previous specified element in the route network graph
                if (!firstElement)
                {
                    if (!currentElement.NeighborElements.Exists(n => n == previousElement))
                        throw new ArgumentException("Route element id: " + routeElementId + " at position: " + position + " (in the route element ids property) is not neighboor to previous id: " + previousElementId + " according to the route network graph. Please check that the walk is valid.");
                }

                position++;
                firstElement = false;
                previousElementId = routeElementId;
                previousElement = currentElement;
            }

            // Check if first id is a node
            if (!(routeQueryService.GetRouteElementInfo(request.RouteElementIds[0]) is RouteNodeInfo))
            {
                throw new ArgumentException("First route element id (in the route element ids property) must be a node, but it's not.");
            }

            // Check if last id is a node
            if (!(routeQueryService.GetRouteElementInfo(request.RouteElementIds[request.RouteElementIds.Count - 1]) is RouteNodeInfo))
            {
                throw new ArgumentException("Last route element id (in the route element ids property) must be a node, but it's not.");
            }


            var walkOfInterest = new WalkOfInterest(request.WalkOfInterestId, request.RouteElementIds);
            repo.Store(walkOfInterest);

            return Unit.Task;
        }
    }
}
