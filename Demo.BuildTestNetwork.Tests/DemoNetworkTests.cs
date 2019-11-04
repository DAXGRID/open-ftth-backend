using Microsoft.Extensions.DependencyInjection;
using ConduitNetwork.Business.Tests.Common;
using RouteNetwork.QueryService;
using System;
using Xunit;
using System.Linq;
using ConduitNetwork.QueryService;

namespace Demo.BuildTestNetwork.Tests
{
    public class DemoNetworkTests : IClassFixture<ContainerFixture>
    {
        private ContainerFixture serviceContext;

        public DemoNetworkTests(ContainerFixture containerFixture)
        {
            this.serviceContext = containerFixture;
        }

        [Fact]
        public void QueryRouteElementsTest()
        {
            var routeQueryService = serviceContext.ServiceProvider.GetService<IRouteNetworkState>();

            // Try query some nodes
            var nodes = routeQueryService.GetAllRouteNodes().ToList();
            Assert.True(nodes.Count > 10);

            Assert.NotNull(nodes[0].Geometry);
            Assert.NotNull(nodes[0].Geometry.GeoJsonType);
            Assert.NotNull(nodes[0].Geometry.GeoJsonCoordinates);

            // Try query some segments
            var segments = routeQueryService.GetAllRouteSegments().ToList();
            Assert.True(segments.Count > 10);

            Assert.NotNull(segments[0].Geometry);
            Assert.NotNull(segments[0].Geometry.GeoJsonType);
            Assert.NotNull(segments[0].Geometry.GeoJsonCoordinates);
        }

        [Fact]
        public void QueryConduitElementsTest()
        {
            var routeQueryService = serviceContext.ServiceProvider.GetService<IRouteNetworkState>();
            var conduitNetworkQueryService = serviceContext.ServiceProvider.GetService<IConduitNetworkQueryService>();

            var routeNodes = routeQueryService.GetAllRouteNodes().ToList();

            //  try query all routeNodes (to find out if read model might give expections)
            foreach (var routeNode in routeNodes)
                conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(routeNode.Id);

            var cabinet1010 = routeNodes.Find(n => n.Name == "SP-1010");
            var cabinetRelatedConduits = conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(cabinet1010.Id);

            var junction1010 = routeNodes.Find(n => n.Name == "J-1010");
            var junctionRelatedConduits = conduitNetworkQueryService.GetConduitSegmentsRelatedToPointOfInterest(junction1010.Id);


        }
    }
}
