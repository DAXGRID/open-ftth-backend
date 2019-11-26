﻿using ConduitNetwork.Business.Commands;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using Core.GraphSupport.Model;
using EquipmentService;
using FiberNetwork.Business.Commands;
using MediatR;
using RouteNetwork.Business.Commands;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo.BuildTestNetwork.Builders
{
    public static class EquipmentBuilder
    {
        public static void Run(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IMediator commandBus)
        {
            var conduitClosureId = Guid.Parse("4ba4e2de-a06d-4168-b85b-a65490a9f313");
            var pointOfInterestId = Guid.Parse("0b2168f2-d9be-455c-a4de-e9169f000122");

            // Create conduit closure in J-1010
            var createConduitClosureCmd = new PlaceConduitClosureCommand()
            {
                ConduitClosureId = conduitClosureId,
                PointOfInterestId = pointOfInterestId
            };

            commandBus.Send(createConduitClosureCmd).Wait();

            // Attached the two flex conduits from J-1010 to SP-1010 to conduit closure
            var attachFlexConduitCmd1 = new AttachConduitEndToClosureCommand()
            {
                ConduitClosureId = conduitClosureId,
                ConduitId = Guid.Parse("9f779e83-9ffd-e5dd-2cfe-cb58197f74b0"),
                Side = ConduitClosureInfoSide.Top
            };

            commandBus.Send(attachFlexConduitCmd1).Wait();

            var attachFlexConduitCmd2 = new AttachConduitEndToClosureCommand()
            {
                ConduitClosureId = conduitClosureId,
                ConduitId = Guid.Parse("a7425e14-ba84-c958-c89f-6e00d84355a4"),
                Side = ConduitClosureInfoSide.Top
            };

            commandBus.Send(attachFlexConduitCmd2).Wait();

            // Create 96 fiber cable from CO-BDAL to CO-BRED
            var startNode = routeNetworkQueryService.GetRouteNodeInfo(Guid.Parse("0b2168f2-d9be-455c-a4de-e9169f000016"));

            Guid endNodeId = Guid.Parse("0b2168f2-d9be-455c-a4de-e9169f000046");

            List<Guid> result = new List<Guid>();

            List<IGraphElement> graphElements = new List<IGraphElement>();

            graphElements.AddRange(routeNetworkQueryService.GetAllRouteNodes());
            graphElements.AddRange(routeNetworkQueryService.GetAllRouteSegments());

            // remove element, so it has to be routed left through the pf and junctions
            var elementToRemove = graphElements.Find(g => g.Id == Guid.Parse("b95000fb-425d-4cd3-9f45-66e8c5000050"));
            graphElements.Remove(elementToRemove);

            var graph = new Graph(graphElements);

            var shortestPathResult = graph.ShortestPath(startNode.Id.ToString(), endNodeId.ToString()).ToList();

            var cableWalkOfInterest = new List<Guid>();

            foreach (var item in shortestPathResult)
                cableWalkOfInterest.Add(item.Id);

            // Walk of interest for cable
            var registerMultiConduitWalk = new RegisterWalkOfInterestCommand()
            {
                WalkOfInterestId = Guid.NewGuid(),
                RouteElementIds = cableWalkOfInterest
            };

            commandBus.Send(registerMultiConduitWalk).Wait();

            // Place cable
            var placeCable1 = new PlaceFiberCableCommand()
            {
                FiberCableId = Guid.NewGuid(),
                WalkOfInterestId = registerMultiConduitWalk.WalkOfInterestId,
                NumberOfFibers = 96
            };

            commandBus.Send(placeCable1).Wait();

        }
    }
}
