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
using Xunit.Extensions.Ordering;

namespace RouteNetwork.Business.Tests
{
    /// <summary>
    /// To avoid test classes fiddeling with the same event store
    /// </summary>
    public class TypeOneTestNetworkContainerFixture : ContainerFixtureBase
    {
        public TypeOneTestNetworkContainerFixture() : base("type_one_test_network_test") {}
    }

    public class TypeOneRouteNetworkTests : IClassFixture<TypeOneTestNetworkContainerFixture>
    {
        private ContainerFixtureBase container;

        private TestRouteNetworkType1 _testRouteNetwork;

        public TypeOneRouteNetworkTests(TypeOneTestNetworkContainerFixture fixture)
        {
            container = fixture;
            _testRouteNetwork = new TestRouteNetworkType1(container);
        }
    

        [Fact]
        public void TestRouteNetworkBasicTest()
        {
            Assert.Equal("CO-1", _testRouteNetwork.GetNodeByName("CO-1").Name);
            Assert.Equal("HH-10", _testRouteNetwork.GetNodeByName("HH-10").Name);
        }

        [Fact]
        public void TestRouteNetworkTraverseTest()
        {
            // Traverse the whole route network, starting at CO-1, to figure out if everything is connected to each other
            var result = _testRouteNetwork.GetNodeByName("CO-1").UndirectionalDFS<RouteNodeInfo, RouteSegmentInfo>().ToList();

            // Check that the traverse reached HH-10
            Assert.True(result.Exists(e => e is RouteNodeInfo && ((RouteNodeInfo)e).Name == "HH-10"));

            // Check that the traverse reached SDU-3
            Assert.True(result.Exists(e => e is RouteNodeInfo && ((RouteNodeInfo)e).Name == "SDU-3"));
            
            // Check that we got total 12 nodes
            Assert.Equal(12, result.Count(e => e is RouteNodeInfo));

            // Check that we got total 11 segments
            Assert.Equal(11, result.Count(e => e is RouteSegmentInfo));
              
            // Check that we find 3 service delivery points (the 3 SDU's created in the initial network)
            Assert.Equal(3, result.Count(e => e is RouteNodeInfo && ((RouteNodeInfo)e).NodeFunctionKind == RouteNodeFunctionKindEnum.ServiceDeliveryPoint));
        }

    }
}
