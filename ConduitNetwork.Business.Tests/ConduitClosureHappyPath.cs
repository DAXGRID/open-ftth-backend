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
using ConduitNetwork.Events.Model;

namespace ConduitNetwork.Business.Tests
{
    public class ConduitClosureContainerFixture : ContainerFixtureBase
    {
        public ConduitClosureContainerFixture() : base("happy_conduit_closure_tests") { }
    }

    [Order(1)]
    public class ConduitClosureHappyPath : IClassFixture<ConduitClosureContainerFixture>
    {
        private ConduitClosureContainerFixture serviceContext;

        private static TestRouteNetworkType1 _testNetwork = null;

        // Commands intially executed to place some conduits for use in tests
        private static PlaceMultiConduitCommand _flatlinerCmd;
        private static PlaceMultiConduitCommand _emetelleCmd;
        private static PlaceSingleConduitCommand _createSingleConduit1Cmd;
        private static PlaceSingleConduitCommand _createSingleConduit2Cmd;
        private static PlaceMultiConduitCommand _createFlexConduitCmd;

        // Command executed in the tests
        private static PlaceConduitClosureCommand _createConduitClosureCmd;

        public ConduitClosureHappyPath(ConduitClosureContainerFixture serviceContext)
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
            _createSingleConduit1Cmd = fixture.Build<PlaceSingleConduitCommand>()
                .With(x => x.WalkOfInterestId, registerSingleConduitWalk.WalkOfInterestId)
                .With(x => x.Name, "R3")
                .With(x => x.DemoDataSpec, "Ø12")
                .Create();

            serviceContext.CommandBus.Send(_createSingleConduit1Cmd).Wait();

            // Create second single conduit
            _createSingleConduit2Cmd = fixture.Build<PlaceSingleConduitCommand>()
                .With(x => x.WalkOfInterestId, registerSingleConduitWalk.WalkOfInterestId)
                .With(x => x.Name, "R4")
                .With(x => x.DemoDataSpec, "Ø12")
                .Create();

            serviceContext.CommandBus.Send(_createSingleConduit2Cmd).Wait();

            // Create flex conduit
            _createFlexConduitCmd = fixture.Build<PlaceMultiConduitCommand>()
                .With(x => x.WalkOfInterestId, registerSingleConduitWalk.WalkOfInterestId)
                .With(x => x.Name, string.Empty)
                .With(x => x.DemoDataSpec, "FLEX-1")
                .Create();

            serviceContext.CommandBus.Send(_createFlexConduitCmd);

        }

        [Fact, Order(1)]
        public void PlaceConduitClosure()
        {
            var conduitClosureRepository = serviceContext.ServiceProvider.GetService<IConduitClosureRepository>();
            
            var fixture = new Fixture();

            _createConduitClosureCmd = fixture.Build<PlaceConduitClosureCommand>()
               .With(x => x.PointOfInterestId, _testNetwork.GetNodeByName("CC-1").Id)
               .Create();

            serviceContext.CommandBus.Send(_createConduitClosureCmd).Wait();

            // Check that read side got updated
            var conduitClosure = conduitClosureRepository.GetConduitClosureInfo(_createConduitClosureCmd.ConduitClosureId);

            Assert.NotNull(conduitClosureRepository);
            Assert.Equal(_createConduitClosureCmd.PointOfInterestId, conduitClosure.PointOfInterestId);

            // Check that the closure has four sides
            Assert.Equal(4, conduitClosure.Sides.Count);

            // Check that the each side has zero ports
            Assert.Equal(0, conduitClosure.Sides.Count(s => s.Ports.Count != 0));

        }

        [Fact, Order(2)]
        public void AttachPassByMultiConduitToClosure()
        {
            var conduitClosureRepository = serviceContext.ServiceProvider.GetService<IConduitClosureRepository>();

            var fixture = new Fixture();

            // Route emetelle multi conduit through closure in direction vest -> east
            var routeMultiConduitThroughClosureCmd = fixture.Build<AttachPassByConduitToClosureCommand>()
               .With(x => x.ConduitClosureId, _createConduitClosureCmd.ConduitClosureId)
               .With(x => x.ConduitId, _emetelleCmd.MultiConduitId)
               .With(x => x.IncommingSide, ConduitClosureInfoSide.Left)
               .With(x => x.IncommingPortPosition, 0)
               .With(x => x.OutgoingSide, ConduitClosureInfoSide.Right)
               .With(x => x.OutgoingPortPosition, 0)
               .Create();

            serviceContext.CommandBus.Send(routeMultiConduitThroughClosureCmd).Wait();

            var conduitClosure = conduitClosureRepository.GetConduitClosureInfo(_createConduitClosureCmd.ConduitClosureId);

            // Check that left side port 1 is connected to right side port 1, type pass through
            var leftSide = conduitClosure.Sides.Find(s => s.Position == Events.Model.ConduitClosureInfoSide.Left);
            var leftPort = leftSide.Ports.Find(p => p.Position == 1);
            Assert.Equal(ConduitClosureInfoSide.Right, leftPort.ConnectedToSide);
            Assert.Equal(1, leftPort.ConnectedToPort);
            Assert.Equal(ConduitClosureInternalConnectionKindEnum.PassThrough, leftPort.ConnectionKind);

            // Check that right side port 1 is connected to left side port 1, and type is pass through
            var rightSide = conduitClosure.Sides.Find(s => s.Position == Events.Model.ConduitClosureInfoSide.Right);
            var rightPort = rightSide.Ports.Find(p => p.Position == 1);
            Assert.Equal(ConduitClosureInfoSide.Left, rightPort.ConnectedToSide);
            Assert.Equal(1, rightPort.ConnectedToPort);
            Assert.Equal(ConduitClosureInternalConnectionKindEnum.PassThrough, rightPort.ConnectionKind);

            // Check that multi conduit segment reference is resolved
            Assert.NotNull(leftPort.MultiConduitSegment);
            Assert.NotNull(rightPort.MultiConduitSegment);

            // Check that each side has got 7 terminals (because it's a multi conduit with 7 inner conduits attached)
            Assert.Equal(7, leftPort.Terminals.Count);
            Assert.Equal(7, leftPort.Terminals.Count);

            // Check terminals on left port
            Assert.Equal(7, leftPort.Terminals.Count(t => t.LineId != Guid.Empty));
            Assert.Equal(7, leftPort.Terminals.Count(t => t.LineSegmentEndKind == ConduitEndKindEnum.Incomming));
            Assert.Equal(7, leftPort.Terminals.Count(t => t.ConnectedToSide == ConduitClosureInfoSide.Right));
            Assert.Equal(7, leftPort.Terminals.Count(t => t.ConnectedToPort > 0));
            Assert.Equal(7, leftPort.Terminals.Count(t => t.ConnectedToTerminal > 0));
            Assert.Equal(7, leftPort.Terminals.Count(t => t.LineSegment != null));

            // Check terminals on right port
            Assert.Equal(7, rightPort.Terminals.Count(t => t.LineId != Guid.Empty));
            Assert.Equal(7, rightPort.Terminals.Count(t => t.LineSegmentEndKind == ConduitEndKindEnum.Outgoing));
            Assert.Equal(7, rightPort.Terminals.Count(t => t.ConnectedToSide == ConduitClosureInfoSide.Left));
            Assert.Equal(7, rightPort.Terminals.Count(t => t.ConnectedToPort > 0));
            Assert.Equal(7, rightPort.Terminals.Count(t => t.ConnectedToTerminal > 0));
            Assert.Equal(7, rightPort.Terminals.Count(t => t.LineSegment != null));


        }

        [Fact, Order(3)]
        public void CutOuterConduitInClosure()
        {
            var conduitClosureRepository = serviceContext.ServiceProvider.GetService<IConduitClosureRepository>();

            var fixture = new Fixture();

            // cut outer conduit of emetelle multi conduit
            var cutOuterConduit = fixture.Build<CutOuterConduitCommand>()
               .With(x => x.MultiConduitId, _emetelleCmd.MultiConduitId)
               .With(x => x.PointOfInterestId, _testNetwork.GetNodeByName("CC-1").Id)
               .Create();

            serviceContext.CommandBus.Send(cutOuterConduit).Wait();

            var conduitClosure = conduitClosureRepository.GetConduitClosureInfo(_createConduitClosureCmd.ConduitClosureId);

            // Check that left port now has connection kind = connected
            var leftSide = conduitClosure.Sides.Find(s => s.Position == Events.Model.ConduitClosureInfoSide.Left);
            var leftPort = leftSide.Ports.Find(p => p.Position == 1);
            Assert.True(leftPort.ConnectionKind == ConduitClosureInternalConnectionKindEnum.NotConnected);
            Assert.True(leftPort.ConnectedToSide == 0);
            Assert.True(leftPort.ConnectedToPort == 0);

            // Check that right port now has connection kind = connected
            var rightSide = conduitClosure.Sides.Find(s => s.Position == Events.Model.ConduitClosureInfoSide.Right);
            var rightPort = rightSide.Ports.Find(p => p.Position == 1);
            Assert.True(rightPort.ConnectionKind == ConduitClosureInternalConnectionKindEnum.NotConnected);

            // Check that right and left port point to two different conduit segments
            Assert.True(rightPort.MultiConduitSegmentId != leftPort.MultiConduitSegmentId);
            Assert.True(rightPort.ConnectedToSide == 0);
            Assert.True(rightPort.ConnectedToPort == 0);
        }


        [Fact, Order(4)]
        public void CutInnerConduitInClosure()
        {
            var conduitClosureRepository = serviceContext.ServiceProvider.GetService<IConduitClosureRepository>();

            var fixture = new Fixture();

            // cut inner conduit number 2 of emetelle multi conduit
            var cutOuterConduit = fixture.Build<CutInnerConduitCommand>()
               .With(x => x.MultiConduitId, _emetelleCmd.MultiConduitId)
               .With(x => x.InnerConduitSequenceNumber, 2)
               .With(x => x.PointOfInterestId, _testNetwork.GetNodeByName("CC-1").Id)
               .Create();

            serviceContext.CommandBus.Send(cutOuterConduit).Wait();

            var conduitClosure = conduitClosureRepository.GetConduitClosureInfo(_createConduitClosureCmd.ConduitClosureId);

            // Check left port
            var leftSide = conduitClosure.Sides.Find(s => s.Position == Events.Model.ConduitClosureInfoSide.Left);
            var leftPort = leftSide.Ports.Find(p => p.Position == 1);
            var leftTerminal = leftPort.Terminals.Find(t => t.Position == 2);

            // check that terminal cut has connection kind = not connected
            Assert.True(leftTerminal.ConnectionKind == ConduitClosureInternalConnectionKindEnum.NotConnected);

            // Check that the other 6 terminals still has connection kind = PassThrough
            Assert.Equal(6, leftPort.Terminals.Count(t => t != leftTerminal && t.ConnectionKind == ConduitClosureInternalConnectionKindEnum.PassThrough));

            // Check right port
            var rightSide = conduitClosure.Sides.Find(s => s.Position == Events.Model.ConduitClosureInfoSide.Right);
            var rightPort = rightSide.Ports.Find(p => p.Position == 1);
            var rightTerminal = rightPort.Terminals.Find(t => t.Position == 2);

            // check that terminal cut has connection kind = not connected
            Assert.True(rightTerminal.ConnectionKind == ConduitClosureInternalConnectionKindEnum.NotConnected);
            // check that terminal connected to info is cleared
            Assert.True(rightTerminal.ConnectedToPort == 0);
            Assert.True(rightTerminal.ConnectedToSide == 0);
            Assert.True(rightTerminal.ConnectedToTerminal == 0);

            // Check that the other 6 terminals still has connection kind = PassThrough
            Assert.Equal(6, rightPort.Terminals.Count(t => t != rightTerminal && t.ConnectionKind == ConduitClosureInternalConnectionKindEnum.PassThrough));
        }

        [Fact, Order(4)]
        public void AttachSingleConduitsToClosure()
        {
            var conduitClosureRepository = serviceContext.ServiceProvider.GetService<IConduitClosureRepository>();

            var fixture = new Fixture();

            // Attach the first single conduit to the top of the closure
            var attachSingleConduit1Cmd = fixture.Build<AttachConduitEndToClosureCommand>()
               .With(x => x.ConduitClosureId, _createConduitClosureCmd.ConduitClosureId)
               .With(x => x.ConduitId, _createSingleConduit1Cmd.SingleConduitId)
               .With(x => x.Side, ConduitClosureInfoSide.Top)
               .With(x => x.PortPosition, 0)
               .With(x => x.TerminalPosition, 0)
               .Create();

            serviceContext.CommandBus.Send(attachSingleConduit1Cmd).Wait();

            var conduitClosure = conduitClosureRepository.GetConduitClosureInfo(_createConduitClosureCmd.ConduitClosureId);

            var topSide = conduitClosure.Sides.Find(s => s.Position == Events.Model.ConduitClosureInfoSide.Top);
            var topPort = topSide.Ports.Find(p => p.Position == 1);
            var topTerminal = topPort.Terminals.Find(t => t.Position == 1);

            // Check port
            Assert.Equal(Guid.Empty, topPort.MultiConduitId);
            Assert.Equal(ConduitClosureInternalConnectionKindEnum.NotConnected, topPort.ConnectionKind);

            // Check terminal
            Assert.NotNull(topTerminal.LineSegment);
            Assert.Equal(ConduitClosureInternalConnectionKindEnum.NotConnected, topTerminal.ConnectionKind);
            Assert.Equal(_createSingleConduit1Cmd.SingleConduitId, topTerminal.LineId);

        }

        [Fact, Order(5)]
        public void ConnectSingleConduitToInnerConduit()
        {
            var conduitClosureRepository = serviceContext.ServiceProvider.GetService<IConduitClosureRepository>();

            var fixture = new Fixture();

            // Connect single conduit to junction
            var connectSingleConduitToJunctionCmd = fixture.Build<ConnectSingleConduitToJunction>()
               .With(x => x.SingleConduitId, _createSingleConduit1Cmd.SingleConduitId)
               .With(x => x.PointOfInterestId, _testNetwork.GetNodeByName("CC-1").Id)
               .With(x => x.ConnectedEndKind, ConduitEndKindEnum.Outgoing)
               .Create();

            serviceContext.CommandBus.Send(connectSingleConduitToJunctionCmd).Wait();

            // Connect emetelle inner conduit 2 to the same junction
            var connectInnerConduitToJunctionCmd = fixture.Build<ConnectInnerConduitToJunction>()
               .With(x => x.MultiConduitId, _emetelleCmd.MultiConduitId)
               .With(x => x.InnerConduitSequenceNumber, 2)
               .With(x => x.PointOfInterestId, _testNetwork.GetNodeByName("CC-1").Id)
               .With(x => x.ConnectedEndKind, ConduitEndKindEnum.Incomming)
               .With(x => x.ConnectedJunctionId, connectSingleConduitToJunctionCmd.ConnectedJunctionId) // Notice the same junction id
               .Create();

            serviceContext.CommandBus.Send(connectInnerConduitToJunctionCmd).Wait();

            var conduitClosure = conduitClosureRepository.GetConduitClosureInfo(_createConduitClosureCmd.ConduitClosureId);

            var topSide = conduitClosure.Sides.Find(s => s.Position == Events.Model.ConduitClosureInfoSide.Top);
            var topPort = topSide.Ports.Find(p => p.Position == 1);
            var topTerminal = topPort.Terminals.Find(t => t.Position == 1);

            // Check top port
            Assert.Equal(Guid.Empty, topPort.MultiConduitId);
            Assert.Equal(ConduitClosureInternalConnectionKindEnum.NotConnected, topPort.ConnectionKind);

            // Check top terminal
            Assert.Equal(ConduitClosureInternalConnectionKindEnum.Connected, topTerminal.ConnectionKind);
            Assert.Equal(_createSingleConduit1Cmd.SingleConduitId, topTerminal.LineId);
            Assert.Equal(ConduitClosureInfoSide.Left, topTerminal.ConnectedToSide);
            Assert.Equal(1, topTerminal.ConnectedToPort);
            Assert.Equal(2, topTerminal.ConnectedToTerminal);

            var leftSide = conduitClosure.Sides.Find(s => s.Position == Events.Model.ConduitClosureInfoSide.Left);
            var leftPort = leftSide.Ports.Find(p => p.Position == 1);
            var leftTerminal = leftPort.Terminals.Find(t => t.Position == 2);

            // Check left port
            Assert.Equal(Guid.Empty, topPort.MultiConduitId);
            Assert.Equal(ConduitClosureInternalConnectionKindEnum.NotConnected, leftPort.ConnectionKind);

            // Check lef terminal
            Assert.Equal(ConduitClosureInternalConnectionKindEnum.Connected, leftTerminal.ConnectionKind);
            Assert.Equal(ConduitClosureInfoSide.Top, leftTerminal.ConnectedToSide);
            Assert.Equal(1, leftTerminal.ConnectedToPort);
            Assert.Equal(1, leftTerminal.ConnectedToTerminal);


        }

        [Fact, Order(6)]
        public void FlexConduitToClosureTest()
        {
            var conduitClosureRepository = serviceContext.ServiceProvider.GetService<IConduitClosureRepository>();

            var fixture = new Fixture();

            // Create conduit closure
            if (_createConduitClosureCmd == null)
            {
                _createConduitClosureCmd = fixture.Build<PlaceConduitClosureCommand>()
                    .With(x => x.PointOfInterestId, _testNetwork.GetNodeByName("CC-1").Id)
                    .Create();

                serviceContext.CommandBus.Send(_createConduitClosureCmd).Wait();
            }

            // Attach flex conduit to the top of the closure
            var attachFlexConduit = fixture.Build<AttachConduitEndToClosureCommand>()
               .With(x => x.ConduitClosureId, _createConduitClosureCmd.ConduitClosureId)
               .With(x => x.ConduitId, _createFlexConduitCmd.MultiConduitId)
               .With(x => x.Side, ConduitClosureInfoSide.Top)
               .With(x => x.PortPosition, 0)
               .With(x => x.TerminalPosition, 0)
               .Create();

            serviceContext.CommandBus.Send(attachFlexConduit).Wait();

            var conduitClosure = conduitClosureRepository.GetConduitClosureInfo(_createConduitClosureCmd.ConduitClosureId);

            var topSide = conduitClosure.Sides.Find(s => s.Position == Events.Model.ConduitClosureInfoSide.Top);
            var topPort = topSide.Ports.Find(p => p.MultiConduitId == _createFlexConduitCmd.MultiConduitId);

            // Check port
            Assert.Equal(_createFlexConduitCmd.MultiConduitId, topPort.MultiConduitId);
            Assert.Equal(ConduitClosureInternalConnectionKindEnum.NotConnected, topPort.ConnectionKind);
            Assert.Empty(topPort.Terminals); // Because the flex conduit have no inner conduits yet, no terminals should exist

            // Add first inner conduit to flex conduit
            var addInnerConduitCmd1 = fixture.Build<AddInnerConduitCommand>()
                .With(x => x.MultiConduitId, _createFlexConduitCmd.MultiConduitId)
                .With(x => x.Color, Events.Model.ConduitColorEnum.Blue)
                .With(x => x.OuterDiameter, 12)
                .With(x => x.InnerDiameter, 10)
                .Create();

            serviceContext.CommandBus.Send(addInnerConduitCmd1);

            // Check that closure port terminal was properly added as part of adding inner conduit
            conduitClosure = conduitClosureRepository.GetConduitClosureInfo(_createConduitClosureCmd.ConduitClosureId);
            topSide = conduitClosure.Sides.Find(s => s.Position == Events.Model.ConduitClosureInfoSide.Top);
            Assert.Single(topSide.Ports); // Top must still have one port

            topPort = topSide.Ports.Find(p => p.MultiConduitId == _createFlexConduitCmd.MultiConduitId);

            Assert.Single(topPort.Terminals); // Must have one terminal now

            var topTerminal = topPort.Terminals.Find(t => t.Position == 1);

            // Add second inner conduit to flex conduit
            var addInnerConduitCmd2 = fixture.Build<AddInnerConduitCommand>()
                .With(x => x.MultiConduitId, _createFlexConduitCmd.MultiConduitId)
                .With(x => x.Color, Events.Model.ConduitColorEnum.Blue)
                .With(x => x.OuterDiameter, 12)
                .With(x => x.InnerDiameter, 10)
                .Create();

            serviceContext.CommandBus.Send(addInnerConduitCmd2);

            // Check that closure port terminal was properly added as part of adding inner conduit
            conduitClosure = conduitClosureRepository.GetConduitClosureInfo(_createConduitClosureCmd.ConduitClosureId);
            topSide = conduitClosure.Sides.Find(s => s.Position == Events.Model.ConduitClosureInfoSide.Top);
            Assert.Single(topSide.Ports); // Top must still have one port

            topPort = topSide.Ports.Find(p => p.MultiConduitId == _createFlexConduitCmd.MultiConduitId);

            Assert.Equal(2, topPort.Terminals.Count); // Must have two terminal now


        }

    }
}
