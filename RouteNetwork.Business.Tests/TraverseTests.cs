using AutoFixture;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using RouteNetwork.Business.Aggregates;
using RouteNetwork.Business.Commands;
using RouteNetwork.Business.Tests.Common;
using RouteNetwork.Events.Model;
using RouteNetwork.Projections;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Linq;
using Xunit;

namespace RouteNetwork.Business.Tests
{
    /// <summary>
    /// To avoid test classes fiddeling with the same event store, because they clean it etc.
    /// </summary>
    public class TraversalContainerFixture : ContainerFixtureBase
    {
        public TraversalContainerFixture() : base("route_network_traversal") { }
    }

    [Collection("Sequential")]
    public class TraverseTests : IClassFixture<TraversalContainerFixture>
    {
        private ContainerFixtureBase container;

        private Guid testCabinet1;
        private Guid testJunction1;
        private Guid testSdu1;
        private Guid testSdu2;


        public TraverseTests(TraversalContainerFixture containerFixture)
        {
            container = containerFixture;

            // Create a little network for testing
            //
            // cabinet1 ---- sdu1
            //           |-- junction1 -- sdu2

            var fixture = new Fixture();

            // Cabinet 1
            var cabinet1Cmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Name, "cabinet 1")
                .With(x => x.Geometry, new Geometry("Point", "[10, 10]"))
                .With(x => x.NodeKind, RouteNodeKindEnum.CabinetSmall)
                .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.SplicePoint)
                .Create();

            container.CommandBus.Send(cabinet1Cmd).Wait();
            testCabinet1 = cabinet1Cmd.Id;

            // SDU 1
            var sdu1cmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Name, "sdu 1")
              .With(x => x.Geometry, new Geometry("Point", "[10, 11]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.SingleDwellingUnit)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.ServiceDeliveryPoint)
              .Create();

            container.CommandBus.Send(sdu1cmd).Wait();
            testSdu1 = sdu1cmd.Id;

            // Junction 1
            var junction1cmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Name, "junction 1")
              .With(x => x.Geometry, new Geometry("Point", "[11, 10]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.ConduitClosure)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.BurriedConduitPont)
              .Create();

            container.CommandBus.Send(junction1cmd).Wait();
            testJunction1 = junction1cmd.Id;

            // SDU 2
            var sdu2cmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Name, "sdu 2")
              .With(x => x.Geometry, new Geometry("Point", "[12, 10]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.SingleDwellingUnit)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.ServiceDeliveryPoint)
              .Create();

            container.CommandBus.Send(sdu2cmd).Wait();
            testSdu2 = sdu2cmd.Id;

            // Add segment between cabinet 1 and sdu 1
            var addSegment1Cmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, cabinet1Cmd.Id)
              .With(x => x.ToNodeId, sdu1cmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[10,10],[10, 11]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            container.CommandBus.Send(addSegment1Cmd).Wait();


            // Add segment between cabinet 1 and junction 1
            var addSegment2Cmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, cabinet1Cmd.Id)
              .With(x => x.ToNodeId, junction1cmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[10,10],[11, 10]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            container.CommandBus.Send(addSegment2Cmd).Wait();


            // Add segment between junction 1 and sdu 2
            var addSegment3Cmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, junction1cmd.Id)
              .With(x => x.ToNodeId, sdu2cmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[11,10],[12, 10]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            container.CommandBus.Send(addSegment3Cmd).Wait();
        }


        [Fact]
        public void TraverseEntireNetworkTest()
        {
            var routeNetworkQueryService = container.ServiceProvider.GetService<IRouteNetworkState>();

            var cabinet1 = routeNetworkQueryService.GetRouteNodeInfo(testCabinet1);

            // Traverse whole network (no predicate)
            var result = cabinet1.UndirectionalDFS<RouteNodeInfo, RouteSegmentInfo>();

            // Expect 4 nodes and 3 segment - that's 7 in total
            Assert.Equal(7, result.Count());
        }

        [Fact]
        public void TraverseFromCabinetUntilJunction()
        {
            var routeNetworkQueryService = container.ServiceProvider.GetService<IRouteNetworkState>();

            var cabinet1 = routeNetworkQueryService.GetRouteNodeInfo(testCabinet1);

            var result = cabinet1.UndirectionalDFS<RouteNodeInfo, RouteSegmentInfo>(
                n => n.NodeKind != RouteNodeKindEnum.ConduitClosure
            );

            // Expect 4: 
            // The cabinet
            // The segment from cabinet to junction (and here the traverse will not go futher due to predicate)
            // The segment from cabinet to sdu 1
            // The sdu 1

            Assert.Equal(4, result.Count());
        }

    }
}
