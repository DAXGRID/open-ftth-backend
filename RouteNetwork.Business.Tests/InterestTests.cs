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
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RouteNetwork.Business.Tests
{
    /// <summary>
    /// To avoid test classes fiddeling with the same event store, because they clean it etc.
    /// </summary>
    public class InterestContainerFixture : ContainerFixtureBase
    {
        public InterestContainerFixture() : base("route_network_interest_test") { }
    }

    [Collection("Sequential")]
    public class InterestTests : IClassFixture<InterestContainerFixture>
    {
        private InterestContainerFixture container;

        private Guid testCabinet1;
        private Guid testJunction1;
        private Guid testSdu1;
        private Guid testSdu2;

        private Guid testCabinet1ToJunction1;
        private Guid testCabinet1ToSdu1;
        private Guid testJunction1ToSdu2;
                       
        public InterestTests(InterestContainerFixture interestContainerFixture)
        {
            container = interestContainerFixture;

            var routeNetworkState = container.ServiceProvider.GetService<IRouteNetworkState>();

            var testRouteNetwork = new TestRouteNetworkType1(container.CommandBus, routeNetworkState);

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
            testCabinet1ToSdu1 = addSegment1Cmd.Id;

            // Add segment between cabinet 1 and junction 1
            var addSegment2Cmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, cabinet1Cmd.Id)
              .With(x => x.ToNodeId, junction1cmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[10,10],[11, 10]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            container.CommandBus.Send(addSegment2Cmd).Wait();
            testCabinet1ToJunction1 = addSegment2Cmd.Id;

            // Add segment between junction 1 and sdu 2
            var addSegment3Cmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, junction1cmd.Id)
              .With(x => x.ToNodeId, sdu2cmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[11,10],[12, 10]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            container.CommandBus.Send(addSegment3Cmd).Wait();
            testJunction1ToSdu2 = addSegment3Cmd.Id;
        }


        [Fact]
        public void RegisterValidWalkOfInterestTests()
        {
            var routeNetworkQueryService = container.ServiceProvider.GetService<IRouteNetworkState>();

            var fixture = new Fixture();

            // Register walk from cabinet 1 -> junction 1 -> sdu 2
            var registerWalkCmd = fixture.Build<RegisterWalkOfInterestCommand>()
                .With(x => x.RouteElementIds, new List<Guid> {testCabinet1, testCabinet1ToJunction1, testJunction1, testJunction1ToSdu2, testSdu2 })
                .Create();

            container.CommandBus.Send(registerWalkCmd).Wait();

            // Check that all route elements in register walk command now has walk of interest relation
            foreach (var routeElementId in registerWalkCmd.RouteElementIds)
            {
                var routeElement = routeNetworkQueryService.GetRouteElementInfo(routeElementId);
                Assert.True(routeElement.WalkOfInterests.Exists(w => w.Id == registerWalkCmd.WalkOfInterestId));
            }

            // Check that the other don't have any
            Assert.True(routeNetworkQueryService.GetRouteElementInfo(testCabinet1ToSdu1).WalkOfInterests.Count == 0);
            Assert.True(routeNetworkQueryService.GetRouteElementInfo(testSdu1).WalkOfInterests.Count == 0);

            // Register walk from cabinet 1 -> junction 1 -> cabinet 1 -> sdu 1. Notice that testCabinet1ToJunction1 segment is walked twice!
            var registerWalkCmd2 = fixture.Build<RegisterWalkOfInterestCommand>()
                .With(x => x.RouteElementIds, new List<Guid> { testCabinet1, testCabinet1ToJunction1, testJunction1, testCabinet1ToJunction1, testCabinet1, testCabinet1ToSdu1, testSdu1 })
                .Create();

            container.CommandBus.Send(registerWalkCmd2).Wait();

            // Now the Cabinet1ToSdu1 segment and sdu1 iself must have a walk associated
            Assert.Single(routeNetworkQueryService.GetRouteElementInfo(testCabinet1ToSdu1).WalkOfInterests);
            Assert.Single(routeNetworkQueryService.GetRouteElementInfo(testSdu1).WalkOfInterests);

            // And three in the cabinet1tojunction1 segment, because it was traversed twice in the last walk
            Assert.Equal(3, routeNetworkQueryService.GetRouteElementInfo(testCabinet1ToJunction1).WalkOfInterests.Count);

        }

        [Fact]
        public async void RegisterInvalidWalkOfInterestTests()
        {
            var fixture = new Fixture();

            // We messed ud the second last route element id. Should have been testJunction1ToSdu2
            var registerWalkCmd = fixture.Build<RegisterWalkOfInterestCommand>()
                .With(x => x.RouteElementIds, new List<Guid> { testCabinet1, testCabinet1ToJunction1, testJunction1, testCabinet1ToSdu1, testSdu2 })
                .Create();

            Exception ex = await Assert.ThrowsAsync<ArgumentException>(() => container.CommandBus.Send(registerWalkCmd));
        }
    }
}
