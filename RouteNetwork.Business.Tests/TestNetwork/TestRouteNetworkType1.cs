using AutoFixture;
using MediatR;
using RouteNetwork.Business.Commands;
using RouteNetwork.Business.Tests.Common;
using RouteNetwork.Events.Model;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;


// This helper class creates an initial test route network like this:
//
//  Y
//
//
//  4                             (J-2)---(SDU-3)
//                                  |
//                                  |
//  3                    (SDU-1)--(J-1)---(SDU-2)
//                                  |
//                                  |
//  2  (CO-1)   (FP-2)            (SP-1)
//       |        |                 |
//       |        |                 |
//  1  (HH-1) - (HH-2) -----------(CC-1)-------------------------------------(HH-10)
//
//       1        2        3        5        6        7        8        9       X


namespace RouteNetwork.Business.Tests
{
    public class TestRouteNetworkType1
    {
        private IMediator _bus;
        private IRouteNetworkQueryService _queryService;

        private Dictionary<string, RouteNodeInfo> _nodesByName = new Dictionary<string, RouteNodeInfo>();
        private Dictionary<string, RouteSegmentInfo> _segmentsByName = new Dictionary<string, RouteSegmentInfo>();

        public TestRouteNetworkType1(ContainerFixtureBase container)
        {
            this._bus = container.CommandBus;
            this._queryService = container.ServiceProvider.GetService<IRouteNetworkQueryService>();

            BuildInitalNetwork();
        }
        
        public RouteNodeInfo GetNodeByName(string name)
        {
            return _nodesByName[name];
        }

        public RouteSegmentInfo GetSegmentByName(string name)
        {
            return _segmentsByName[name];
        }

        private void BuildInitalNetwork()
        {
            // Create all the route nodes first
            CreateNode("CO-1", 1, 2, RouteNodeKindEnum.CentralOfficeSmall, RouteNodeFunctionKindEnum.SecondaryNode);
            CreateNode("HH-1", 1, 1, RouteNodeKindEnum.HandHole, RouteNodeFunctionKindEnum.OpenConduitPoint);
            CreateNode("FP-2", 2, 2, RouteNodeKindEnum.CabinetBig, RouteNodeFunctionKindEnum.FlexPoint);
            CreateNode("HH-2", 2, 1, RouteNodeKindEnum.HandHole, RouteNodeFunctionKindEnum.OpenConduitPoint);
            CreateNode("CC-1", 5, 1, RouteNodeKindEnum.ConduitClosure, RouteNodeFunctionKindEnum.BurriedConduitPont);
            CreateNode("SP-1", 5, 2, RouteNodeKindEnum.CabinetSmall, RouteNodeFunctionKindEnum.SplicePoint);
            CreateNode("J-1", 5, 3, RouteNodeKindEnum.ConduitSimpleJunction, RouteNodeFunctionKindEnum.BurriedConduitPont);
            CreateNode("J-2", 5, 4, RouteNodeKindEnum.ConduitSimpleJunction, RouteNodeFunctionKindEnum.BurriedConduitPont);
            CreateNode("SDU-1", 3, 3, RouteNodeKindEnum.SingleDwellingUnit, RouteNodeFunctionKindEnum.ServiceDeliveryPoint);
            CreateNode("SDU-2", 6, 3, RouteNodeKindEnum.SingleDwellingUnit, RouteNodeFunctionKindEnum.ServiceDeliveryPoint);
            CreateNode("SDU-3", 5, 4, RouteNodeKindEnum.SingleDwellingUnit, RouteNodeFunctionKindEnum.ServiceDeliveryPoint);
            CreateNode("HH-10", 10, 1, RouteNodeKindEnum.HandHole, RouteNodeFunctionKindEnum.OpenConduitPoint);

            // Create the route segment
            CreateSegment("CO-1", "HH-1");
            CreateSegment("HH-1", "HH-2");
            CreateSegment("FP-2", "HH-2");
            CreateSegment("HH-2", "CC-1");
            CreateSegment("CC-1", "SP-1");
            CreateSegment("SP-1", "J-1");
            CreateSegment("J-1", "J-2");
            CreateSegment("J-1", "SDU-1");
            CreateSegment("J-1", "SDU-2");
            CreateSegment("J-2", "SDU-3");
            CreateSegment("CC-1", "HH-10");
        }

        private void CreateNode(string name, int xCoord, int yCoord, RouteNodeKindEnum nodeKind, RouteNodeFunctionKindEnum nodeFunction)
        {
            var fixture = new Fixture();

            var addNodeCmd = fixture.Build<AddNodeCommand>()
              .With(x => x.Name, name)
              .With(x => x.Geometry, new Geometry("Point", "[" + xCoord + "," + yCoord +"]"))
              .With(x => x.NodeKind, nodeKind)
              .With(x => x.NodeFunctionKind, nodeFunction)
              .Create();
            
            _bus.Send(addNodeCmd);

            _nodesByName[name] = _queryService.GetRouteNodeInfo(addNodeCmd.Id);
        }

        private void CreateSegment(string fromName, string toName)
        {
            RouteNodeInfo fromNode = _nodesByName[fromName];
            RouteNodeInfo toNode = _nodesByName[toName];

            var fixture = new Fixture();

            // Add segment between the two nodes
            var addSegmentCmd = fixture.Build<AddSegmentCommand>()
              .With(x => x.FromNodeId, fromNode.Id)
              .With(x => x.ToNodeId, toNode.Id)
              .With(x => x.Geometry, new Geometry("LineString", "[" + fromNode.Geometry.GeoJsonCoordinates + "," + toNode.Geometry.GeoJsonCoordinates + "]"))
              .With(x => x.SegmentKind, RouteSegmentKindEnum.Underground)
              .Create();

            _bus.Send(addSegmentCmd);

            _segmentsByName[fromName + "_" + toName] = _queryService.GetRouteSegmentInfo(addSegmentCmd.Id);
        }
    }
}
