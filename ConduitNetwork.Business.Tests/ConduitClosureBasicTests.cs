using Microsoft.Extensions.DependencyInjection;
using AutoFixture;
using ConduitNetwork.Business.Commands;
using ConduitNetwork.Business.Tests.Common;
using Infrastructure.EventSourcing;
using MediatR;
using System;
using Xunit;
using ConduitNetwork.Business.Aggregates;
using System.Linq;
using RouteNetwork.Business.Commands;
using RouteNetwork.Events.Model;
using System.Collections.Generic;
using RouteNetwork.QueryService;
using ConduitNetwork.ReadModel;
using ConduitNetwork.QueryService;
using Xunit.Extensions.Ordering;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel.ConduitClosure;

namespace ConduitNetwork.Business.Tests
{
    public class ConduitClosureBasicTestsContainerFixture : ContainerFixtureBase
    {
        public ConduitClosureBasicTestsContainerFixture() : base("happy_conduit_closure_tests") { }
    }

    [Order(1)]
    public class ConduitClosureBasicTests : IClassFixture<ConduitClosureBasicTestsContainerFixture>
    {
        private ConduitClosureBasicTestsContainerFixture serviceContext;

        private static TestRouteNetworkType1 _testNetwork = null;

        // Commands intially executed to place some conduits for use in tests
        private static PlaceMultiConduitCommand _flatlinerCmd;
        private static PlaceMultiConduitCommand _emetelleCmd;

        public ConduitClosureBasicTests(ConduitClosureBasicTestsContainerFixture serviceContext)
        {
            this.serviceContext = serviceContext;

            // Create basic route test network, if not already created
            if (_testNetwork != null)
                return;

            _testNetwork = new TestRouteNetworkType1(serviceContext);


            // Now we're going to put two multi conduits from HH-1 to HH-10
            // And two single conduits between CC-1 and SP-1

            var fixture = new Fixture();

            // Walk of interest for placing the two multi conduits
            var registerMultiConduitWalk = fixture.Build<RegisterWalkOfInterestCommand>()
                .With(x => x.RouteElementIds, new List<Guid> {
                    _testNetwork.GetNodeByName("HH-1").Id,
                    _testNetwork.GetSegmentByName("HH-1_HH-2").Id,
                    _testNetwork.GetNodeByName("HH-2").Id,
                    _testNetwork.GetSegmentByName("HH-2_CC-1").Id,
                    _testNetwork.GetNodeByName("CC-1").Id,
                    _testNetwork.GetSegmentByName("CC-1_HH-10").Id,
                    _testNetwork.GetNodeByName("HH-10").Id
                })
                .Create();

            serviceContext.CommandBus.Send(registerMultiConduitWalk).Wait();

            // Create first multi conduit: GM Plast Flatliner with 12 inner conduits
            _flatlinerCmd = fixture.Build<PlaceMultiConduitCommand>()
                .With(x => x.WalkOfInterestId, registerMultiConduitWalk.WalkOfInterestId)
                .With(x => x.Name, string.Empty)
                .With(x => x.DemoDataSpec, "G12F-1-RD") // Flatliner with 12 inner conduits, conduit name = 1, red marking
                .Create();

            serviceContext.CommandBus.Send(_flatlinerCmd);


            // Create second multi conduit: Emetelle round conduit with 7 inner conduits
            _emetelleCmd = fixture.Build<PlaceMultiConduitCommand>()
                .With(x => x.WalkOfInterestId, registerMultiConduitWalk.WalkOfInterestId)
                .With(x => x.Name, string.Empty)
                .With(x => x.DemoDataSpec, "E7R-2-BK") // Emetelle with 7 inner conduits, conduit name = 2, black marking
                .Create();

            serviceContext.CommandBus.Send(_emetelleCmd);

            
            // Walk of interest for placing the two singe conduits
            var registerSingleConduitWalk = fixture.Build<RegisterWalkOfInterestCommand>()
                .With(x => x.RouteElementIds, new List<Guid> {
                    _testNetwork.GetNodeByName("CC-1").Id,
                    _testNetwork.GetSegmentByName("CC-1_SP-1").Id,
                    _testNetwork.GetNodeByName("SP-1").Id
                })
                .Create();

            serviceContext.CommandBus.Send(registerSingleConduitWalk).Wait();


            // Create first single conduit
            var createSingleConduit1Cmd = fixture.Build<PlaceSingleConduitCommand>()
                .With(x => x.WalkOfInterestId, registerSingleConduitWalk.WalkOfInterestId)
                .With(x => x.Name, "R3")
                .With(x => x.DemoDataSpec, "Ø12")
                .Create();

            serviceContext.CommandBus.Send(createSingleConduit1Cmd).Wait();

            // Create second single conduit
            var createSingleConduit2Cmd = fixture.Build<PlaceSingleConduitCommand>()
                .With(x => x.WalkOfInterestId, registerSingleConduitWalk.WalkOfInterestId)
                .With(x => x.Name, "R4")
                .With(x => x.DemoDataSpec, "Ø12")
                .Create();

            serviceContext.CommandBus.Send(createSingleConduit2Cmd).Wait();

        }

        [Fact]
        public void PlaceAndRemoveConduitClosure()
        {
            var conduitClosureRepository = serviceContext.ServiceProvider.GetService<IConduitClosureRepository>();
            
            var fixture = new Fixture();

            var createConduitClosureCmd = fixture.Build<PlaceConduitClosureCommand>()
               .With(x => x.PointOfInterestId, _testNetwork.GetNodeByName("CC-1").Id)
               .Create();

            serviceContext.CommandBus.Send(createConduitClosureCmd).Wait();

            // Check that is was added to read model
            var conduitClosure = conduitClosureRepository.GetConduitClosureInfo(createConduitClosureCmd.ConduitClosureId);

            Assert.NotNull(conduitClosureRepository);
            Assert.Equal(createConduitClosureCmd.PointOfInterestId, conduitClosure.PointOfInterestId);
            Assert.True(conduitClosureRepository.CheckIfConduitClosureAlreadyExists(createConduitClosureCmd.ConduitClosureId));

            // Check that is was added to document store
            using (var session = serviceContext.Store.LightweightSession())
            {
                var conduitClosures = session.Query<ConduitClosureInfo>();

                Assert.True(conduitClosures.Where(c => c.Id == createConduitClosureCmd.ConduitClosureId).Count() == 1);
            }
            
            // Remove the closure
            var removeConduitClosureCmd = fixture.Build<RemoveConduitClosureCommand>()
              .With(x => x.ConduitClosureId, createConduitClosureCmd.ConduitClosureId)
              .Create();

            serviceContext.CommandBus.Send(removeConduitClosureCmd).Wait();

            // Check that is was removed from the read model
            Assert.False(conduitClosureRepository.CheckIfConduitClosureAlreadyExists(createConduitClosureCmd.ConduitClosureId));

            // Check that is was removed from the document store
            using (var session = serviceContext.Store.LightweightSession())
            {
                var conduitClosures = session.Query<ConduitClosureInfo>();

                Assert.True(conduitClosures.Where(c => c.Id == createConduitClosureCmd.ConduitClosureId).Count() == 0);
            }
        }
    }
}
