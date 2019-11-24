using Microsoft.Extensions.DependencyInjection;
using AutoFixture;
using FiberNetwork.Business.Commands;
using FiberNetwork.Business.Tests.Common;
using FiberNetwork.QueryService;
using RouteNetwork.Business.Commands;
using System;
using System.Collections.Generic;
using Xunit;

namespace FiberNetwork.Business.Tests
{
    public class FiberCableContainerFixture : ContainerFixtureBase
    {
        public FiberCableContainerFixture() : base("fiber_network_editing")
        {
        }
    }
        
    public class FiberCableTests : IClassFixture<FiberCableContainerFixture>
    {
        private FiberCableContainerFixture serviceContext;

        private static TestRouteNetworkType1 _testNetwork = null;

        public FiberCableTests(FiberCableContainerFixture serviceContext)
        {
            this.serviceContext = serviceContext;

            // Create basic route test network, if not already created
            if (_testNetwork != null)
                return;

            _testNetwork = new TestRouteNetworkType1(serviceContext);
        }

        [Fact]
        public void PlaceFiberCable()
        {
            // Place a feeder cable from central office CO-1 to flex point FP-2
            var fixture = new Fixture();

            // Walk of interest for placing the feeder cable
            var registerMultiConduitWalk = fixture.Build<RegisterWalkOfInterestCommand>()
                .With(x => x.RouteElementIds, new List<Guid> {
                    _testNetwork.GetNodeByName("CO-1").Id,
                    _testNetwork.GetSegmentByName("CO-1_HH-1").Id,
                    _testNetwork.GetNodeByName("HH-1").Id,
                    _testNetwork.GetSegmentByName("HH-1_HH-2").Id,
                    _testNetwork.GetNodeByName("HH-2").Id,
                    _testNetwork.GetSegmentByName("HH-2_FP-2").Id,
                    _testNetwork.GetNodeByName("FP-2").Id
                })
                .Create();

            serviceContext.CommandBus.Send(registerMultiConduitWalk).Wait();

            // Place 72 fiber cable
            var placeCable1 = fixture.Build<PlaceFiberCableCommand>()
                .With(x => x.WalkOfInterestId, registerMultiConduitWalk.WalkOfInterestId)
                .With(x => x.NumberOfFibers, 72)
                .Create();

            serviceContext.CommandBus.Send(placeCable1).Wait();

            // Check that cable was registered correctly in the network
            var fiberNetworkQueryService = serviceContext.ServiceProvider.GetService<IFiberNetworkQueryService>();

            var fiberCableInfo = fiberNetworkQueryService.GetFiberCableInfo(placeCable1.FiberCalbeId);
            Assert.NotNull(fiberCableInfo);

        }
    }
}
