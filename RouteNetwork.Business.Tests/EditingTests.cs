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
        public EditingContainerFixture() : base("route_network_editing")
        {
        }
    }

    public class EditingTests : IClassFixture<EditingContainerFixture>
    {
        public TestRouteNetworkType1 testRouteNetwork;
        private ContainerFixtureBase container;

        public EditingTests(EditingContainerFixture fixture)
        {
            container = fixture;

            var routeNetworkState = container.ServiceProvider.GetService<IRouteNetworkState>();

            var testRouteNetwork = new TestRouteNetworkType1(container.CommandBus, routeNetworkState);
        }

        [Fact]
        public void AddNodeCheckProjectionTest()
        {
            var fixture = new Fixture();

            var addFirstNodeCmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Name, "My first node")
                .With(x => x.Geometry, new Geometry("Point", "[10, 10]") )
                .With(x => x.NodeKind, RouteNodeKindEnum.CabinetBig)
                .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.FlexPoint)
                .Create();

            container.CommandBus.Send(addFirstNodeCmd);

            /*
            var routeNode = container.AggregateRepository.Load<RouteNode>(addFirstNodeCmd.Id);

            // Check that a route node aggregate is created
            Assert.Equal(addFirstNodeCmd.Id, routeNode.Id);

            // Check that route node info projection is working
            var firstRouteInfo = container.DocumentSession.Query<RouteNodeInfo>().First(o => o.Id == addFirstNodeCmd.Id);
            Assert.Equal(addFirstNodeCmd.Id, firstRouteInfo.Id);
            Assert.Equal(addFirstNodeCmd.Name, firstRouteInfo.Name);
            Assert.Equal(addFirstNodeCmd.NodeKind, firstRouteInfo.NodeKind);
            Assert.Equal((string)addFirstNodeCmd.Geometry.GeoJsonType, firstRouteInfo.Geometry.GeoJsonType);
            Assert.Equal((string)addFirstNodeCmd.Geometry.GeoJsonCoordinates, firstRouteInfo.Geometry.GeoJsonCoordinates);
            */

            // Check that query service got first node
            var routeNetworkQueryService = container.ServiceProvider.GetService<IRouteNetworkState>();
            Assert.True(routeNetworkQueryService.CheckIfRouteNodeIdExists(addFirstNodeCmd.Id));


            // Add another node
            var addSecondNodeCmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Id, Guid.NewGuid())
              .With(x => x.Name, "My second node")
              .With(x => x.Geometry, new Geometry("Point", "[10, 11]"))
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
            Assert.Equal((string)addSecondNodeCmd.Geometry.GeoJsonType, queryServiceRouteInfo.Geometry.GeoJsonType);
            Assert.Equal((string)addSecondNodeCmd.Geometry.GeoJsonCoordinates, queryServiceRouteInfo.Geometry.GeoJsonCoordinates);
        }

        [Fact]
        public void AddSegmentCheckProjectionTest()
        {
            var routeNetworkQueryService = container.ServiceProvider.GetService<IRouteNetworkState>();

            var fixture = new Fixture();

            var addFirstNodeCmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Name, "My first node")
                .With(x => x.Geometry, new Geometry("Point", "[10, 10]"))
                .With(x => x.NodeKind, RouteNodeKindEnum.CabinetBig)
                .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.FlexPoint)
                .Create();

            container.CommandBus.Send(addFirstNodeCmd);

            // Add another node
            var addSecondNodeCmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Id, Guid.NewGuid())
              .With(x => x.Name, "My second node")
              .With(x => x.Geometry, new Geometry("Point", "[10, 11]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.SingleDwellingUnit)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.ServiceDeliveryPoint)
              .Create();

            container.CommandBus.Send(addSecondNodeCmd);

            // Add segment between the two above created nodes
            var addSegmentCmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.Id, Guid.NewGuid())
              .With(x => x.FromNodeId, addFirstNodeCmd.Id)
              .With(x => x.ToNodeId, addSecondNodeCmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[10,10],[10, 11]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            container.CommandBus.Send(addSegmentCmd);

            // Get segment info from query service to check if projection stuff works as expected
            var routeSegmentInfo = routeNetworkQueryService.GetRouteSegmentInfo(addSegmentCmd.Id);
            Assert.Equal(addSegmentCmd.Id, routeSegmentInfo.Id);
            Assert.Equal(addSegmentCmd.FromNodeId, routeSegmentInfo.FromNodeId);
            Assert.Equal(addSegmentCmd.ToNodeId, routeSegmentInfo.ToNodeId);
            Assert.Equal(addSegmentCmd.SegmentKind, routeSegmentInfo.SegmentKind);
            Assert.Equal((string)addSegmentCmd.Geometry.GeoJsonType, routeSegmentInfo.Geometry.GeoJsonType);
            Assert.Equal((string)addSegmentCmd.Geometry.GeoJsonCoordinates, routeSegmentInfo.Geometry.GeoJsonCoordinates);
        }

        [Fact]
        public async void CreateSegmentWithMissingNodeTest()
        {
            var fixture = new Fixture();

            var addFirstNodeCmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Name, "My first node")
                .With(x => x.Geometry, new Geometry("Point", "[10, 10]"))
                .With(x => x.NodeKind, RouteNodeKindEnum.CabinetBig)
                .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.FlexPoint)
                .Create();

            await container.CommandBus.Send(addFirstNodeCmd);
          

            // Add segment between the two above created nodes
            var addSegmentCmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.Id, Guid.NewGuid())
              .With(x => x.FromNodeId, addFirstNodeCmd.Id)
              .With(x => x.ToNodeId, Guid.NewGuid()) // Just some random guid to simulate a pointer to a non existing node
              .With(x => x.Geometry, new Geometry("LineString", "[[10,10],[10, 11]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            Exception ex = await Assert.ThrowsAsync<ArgumentException>(() => container.CommandBus.Send(addSegmentCmd));
        }

        [Fact]
        public async void HappySplitSegmentTest()
        {
            var fixture = new Fixture();

            var addFirstNodeCmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Id, Guid.NewGuid())
              .With(x => x.Name, "My first node")
              .With(x => x.Geometry, new Geometry("Point", "[9.6388923, 55.7479325]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.CabinetBig)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.FlexPoint)
              .Create();

            await container.CommandBus.Send(addFirstNodeCmd);

            // Add another node
            var addSecondNodeCmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Id, Guid.NewGuid())
              .With(x => x.Name, "My second node")
              .With(x => x.Geometry, new Geometry("Point", "[9.6390660, 55.7479268]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.SingleDwellingUnit)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.ServiceDeliveryPoint)
              .Create();

            await container.CommandBus.Send(addSecondNodeCmd);

            // Add segment between the two above created nodes
            var addSegmentCmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.Id, Guid.NewGuid())
              .With(x => x.FromNodeId, addFirstNodeCmd.Id)
              .With(x => x.ToNodeId, addSecondNodeCmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[9.6388923, 55.7479325],[9.6390660, 55.7479268]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            await container.CommandBus.Send(addSegmentCmd);

            
            // Add node to be used for splitting segment, that is within the allowed distance to a segment
            var happyAddSplitNodeCmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Id, Guid.NewGuid())
              .With(x => x.Name, "My second node")
              .With(x => x.Geometry, new Geometry("Point", "[9.6389834,55.7479289]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.Unknown)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.Unknown)
              .Create();

            await container.CommandBus.Send(happyAddSplitNodeCmd);

            var happySplitSegmentCmd = fixture.Build<SplitSegmentCommand>()
                .With(x => x.SegmentId, addSegmentCmd.Id)
                .With(x => x.NodeId, happyAddSplitNodeCmd.Id)
                .Create();

            await container.CommandBus.Send(happySplitSegmentCmd);
        }

        [Fact]
        public async void SadSplitSegmentTest()
        {
            var fixture = new Fixture();

            var addFirstNodeCmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Id, Guid.NewGuid())
              .With(x => x.Name, "My first node")
              .With(x => x.Geometry, new Geometry("Point", "[9.6388923, 55.7479325]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.CabinetBig)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.FlexPoint)
              .Create();

            await container.CommandBus.Send(addFirstNodeCmd);

            // Add another node
            var addSecondNodeCmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Id, Guid.NewGuid())
              .With(x => x.Name, "My second node")
              .With(x => x.Geometry, new Geometry("Point", "[9.6390660, 55.7479268]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.SingleDwellingUnit)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.ServiceDeliveryPoint)
              .Create();

            await container.CommandBus.Send(addSecondNodeCmd);

            // Add segment between the two above created nodes
            var addSegmentCmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.Id, Guid.NewGuid())
              .With(x => x.FromNodeId, addFirstNodeCmd.Id)
              .With(x => x.ToNodeId, addSecondNodeCmd.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[[9.6388923, 55.7479325],[9.6390660, 55.7479268]]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            await container.CommandBus.Send(addSegmentCmd);

            // Add node to be used for splitting segment, that is outside the allowed distance to a segment (place 1 meter away from the segment)
            var sadAddSplitNodeCmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Id, Guid.NewGuid())
              .With(x => x.Name, "My second node")
              .With(x => x.Geometry, new Geometry("Point", "[9.6388983,55.7479223]"))
              .With(x => x.NodeKind, RouteNodeKindEnum.Unknown)
              .With(x => x.NodeFunctionKind, RouteNodeFunctionKindEnum.Unknown)
              .Create();

            await container.CommandBus.Send(sadAddSplitNodeCmd);

            var sadSplitSegmentCmd = fixture.Build<SplitSegmentCommand>()
                .With(x => x.SegmentId, addSegmentCmd.Id)
                .With(x => x.NodeId, sadAddSplitNodeCmd.Id)
                .Create();

            Exception ex = await Assert.ThrowsAsync<ArgumentException>(() => container.CommandBus.Send(sadSplitSegmentCmd));

            Assert.Contains("not within allowed distance", ex.Message);
        }


        [Fact]
        public async void CreateTwoNodesWithSameIdTest()
        {
            var fixture = new Fixture();

            var firstCmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Name, "My node first with id")
                .With(x => x.Geometry, new Geometry("Point", "[10, 10]"))
                .Create();

            await container.CommandBus.Send(firstCmd);

            var secondCmd = fixture.Build<AddNodeCommand>()
                .With(x => x.Id, firstCmd.Id) // This is not allowed
                .With(x => x.Name, "My node with same id")
                .With(x => x.Geometry, new Geometry("Point", "[10, 10]"))
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
                .With(x => x.Id, Guid.NewGuid())
                .With(x => x.Name, "My node")
                .With(x => x.Geometry, new Geometry("Point", "[[10, 10]]")) // is polyline format, not point
                .Create();

            Exception ex = await Assert.ThrowsAsync<ArgumentException>(() => container.CommandBus.Send(command));

            Assert.Contains("Error parsing geometry", ex.Message);


            // Geometry not set
            command = fixture.Build<AddNodeCommand>()
               .With(x => x.Id, Guid.NewGuid())
               .With(x => x.Name, "My node")
               .Without(x => x.Geometry) // don't set geometry
               .Create();

            ex = await Assert.ThrowsAsync<ArgumentException>(() => container.CommandBus.Send(command));

            Assert.Contains("Geometry is null", ex.Message);
        }
    }
}
