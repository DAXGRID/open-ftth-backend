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
    public class EditingContainerFixture : ContainerFixtureBase
    {
        public EditingContainerFixture() : base("route_network_editing") {}
    }

    public class EditingTests : IClassFixture<EditingContainerFixture>
    {
        private ContainerFixtureBase container;

        public EditingTests(EditingContainerFixture fixture)
        {
            container = fixture;
        }

        [Fact]
        public void AddNodeCheckProjectionTest()
        {
            var fixture = new Fixture();

            var addFirstNodeCmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Name, "My first node")
                .With(x => x.Geometry, new Geometry("Point", "[100, 100]") )
                .With(x => x.NodeKind, RouteNodeKindEnum.CabinetBig)
                .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.FlexPoint)
                .Create();

            container.CommandBus.Send(addFirstNodeCmd);

            var routeNode = container.AggregateRepository.Load<RouteNode>(addFirstNodeCmd.Id);

            // Check multi conduit properties
            Assert.Equal(addFirstNodeCmd.Id, routeNode.Id);


            // Check that route node info projection is working
            var firstRouteInfo = container.DocumentSession.Query<RouteNodeInfo>().First(o => o.Id == addFirstNodeCmd.Id);
            Assert.Equal(addFirstNodeCmd.Id, firstRouteInfo.Id);
            Assert.Equal(addFirstNodeCmd.Name, firstRouteInfo.Name);
            Assert.Equal(addFirstNodeCmd.NodeKind, firstRouteInfo.NodeKind);
            Assert.Equal(addFirstNodeCmd.Geometry.GeoJsonType, firstRouteInfo.Geometry.GeoJsonType);
            Assert.Equal(addFirstNodeCmd.Geometry.GeoJsonCoordinates, firstRouteInfo.Geometry.GeoJsonCoordinates);

            // Check that query service got first node
            var routeNetworkQueryService = container.ServiceProvider.GetService<IRouteNetworkQueryService>();
            Assert.True(routeNetworkQueryService.CheckIfRouteNodeIdExists(addFirstNodeCmd.Id));


            // Add another node
            var addSecondNodeCmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Name, "My second node")
              .With(x => x.Geometry, new Geometry("Point", "[100, 110]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.SingleDwellingUnit)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.ServiceDeliveryPoint)
              .Create();

            container.CommandBus.Send(addSecondNodeCmd);

            // Check that route node info projection is working
            Assert.Equal(1, container.DocumentSession.Query<RouteNodeInfo>().Count(o => o.Id == addSecondNodeCmd.Id));

            // Check that query service got second node and all its info
            Assert.True(routeNetworkQueryService.CheckIfRouteNodeIdExists(addSecondNodeCmd.Id));

            var queryServiceRouteInfo = routeNetworkQueryService.GetRouteNodeInfo(addSecondNodeCmd.Id);
            Assert.Equal(addSecondNodeCmd.Id, queryServiceRouteInfo.Id);
            Assert.Equal(addSecondNodeCmd.Name, queryServiceRouteInfo.Name);
            Assert.Equal(addSecondNodeCmd.NodeKind, queryServiceRouteInfo.NodeKind);
            Assert.Equal(addSecondNodeCmd.Geometry.GeoJsonType, queryServiceRouteInfo.Geometry.GeoJsonType);
            Assert.Equal(addSecondNodeCmd.Geometry.GeoJsonCoordinates, queryServiceRouteInfo.Geometry.GeoJsonCoordinates);
        }

        [Fact]
        public void AddSegmentCheckProjectionTest()
        {
            var routeNetworkQueryService = container.ServiceProvider.GetService<IRouteNetworkQueryService>();

            var fixture = new Fixture();

            var addFirstNodeCmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Name, "My first node")
                .With(x => x.Geometry, new Geometry("Point", "[100, 100]"))
                .With(x => x.NodeKind, RouteNodeKindEnum.CabinetBig)
                .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.FlexPoint)
                .Create();

            container.CommandBus.Send(addFirstNodeCmd);

            // Add another node
            var addSecondNodeCmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Name, "My second node")
              .With(x => x.Geometry, new Geometry("Point", "[100, 110]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.SingleDwellingUnit)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.ServiceDeliveryPoint)
              .Create();

            container.CommandBus.Send(addSecondNodeCmd);

            // Add segment between the two above created nodes
            var addSegmentCmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, addFirstNodeCmd.Id)
              .With(x => x.ToNodeId, addSecondNodeCmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[100,100],[100, 110]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            container.CommandBus.Send(addSegmentCmd);

            // Get segment info from query service to check if projection stuff works as expected
            var routeSegmentInfo = routeNetworkQueryService.GetRouteSegmentInfo(addSegmentCmd.Id);
            Assert.Equal(addSegmentCmd.Id, routeSegmentInfo.Id);
            Assert.Equal(addSegmentCmd.FromNodeId, routeSegmentInfo.FromNodeId);
            Assert.Equal(addSegmentCmd.ToNodeId, routeSegmentInfo.ToNodeId);
            Assert.Equal(addSegmentCmd.SegmentKind, routeSegmentInfo.SegmentKind);
            Assert.Equal(addSegmentCmd.Geometry.GeoJsonType, routeSegmentInfo.Geometry.GeoJsonType);
            Assert.Equal(addSegmentCmd.Geometry.GeoJsonCoordinates, routeSegmentInfo.Geometry.GeoJsonCoordinates);
        }

        [Fact]
        public async void CreateSegmentWithMissingNodeTest()
        {
            var fixture = new Fixture();

            var addFirstNodeCmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Name, "My first node")
                .With(x => x.Geometry, new Geometry("Point", "[100, 100]"))
                .With(x => x.NodeKind, RouteNodeKindEnum.CabinetBig)
                .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.FlexPoint)
                .Create();

            await container.CommandBus.Send(addFirstNodeCmd);
          

            // Add segment between the two above created nodes
            var addSegmentCmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, addFirstNodeCmd.Id)
              .With(x => x.ToNodeId, Guid.NewGuid()) // Just some random guid to simulate a pointer to a non existing node
              .With(x => x.Geometry, new Geometry("LineString", "[[100,100],[100, 110]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            Exception ex = await Assert.ThrowsAsync<ArgumentException>(() => container.CommandBus.Send(addSegmentCmd));

            Assert.Contains("Cannot find to node", ex.Message);
        }

        [Fact]
        public async void CreateTwoNodesWithSameIdTest()
        {
            var fixture = new Fixture();

            var firstCmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Name, "My node")
                .With(x => x.Geometry, new Geometry("Point", "[100, 100]"))
                .Create();

            await container.CommandBus.Send(firstCmd);

            var secondCmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Id, firstCmd.Id) // This is not allowed
                .With(x => x.Name, "My node")
                .With(x => x.Geometry, new Geometry("Point", "[100, 100]"))
                .Create();

            Exception ex = await Assert.ThrowsAsync<ArgumentException>(() => container.CommandBus.Send(secondCmd));

            Assert.Contains("already exists", ex.Message);
        }

        [Fact]
        public async void CreateNodesWithBadGeometryTest()
        {
            var fixture = new Fixture();

            // Bad geojson string
            var command = fixture.Build<AddNodeCommand>()
                .With(x => x.Name, "My node")
                .With(x => x.Geometry, new Geometry("Point", "[[100, 100]]")) // is polyline format, not point
                .Create();

            Exception ex = await Assert.ThrowsAsync<ArgumentException>(() => container.CommandBus.Send(command));

            Assert.Contains("Error parsing geometry", ex.Message);


            // Geometry not set
            command = fixture.Build<AddNodeCommand>()
               .With(x => x.Name, "My node")
               .Without(x => x.Geometry) // don't set geometry
               .Create();

            ex = await Assert.ThrowsAsync<ArgumentException>(() => container.CommandBus.Send(command));

            Assert.Contains("Geometry is null", ex.Message);
        }
    }
}
