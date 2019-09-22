using Location.Model;
using MediatR;
using MemoryGraph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RouteNetwork.Business.Commands;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Demo.BuildTestNetwork.Builders
{
    public static class RouteNetworkBuilder
    {
        public static Graph _routeGraph = new Graph();
        public static Dictionary<string, string[]> _nodeBuildCodes = new Dictionary<string, string[]>();
        public static Dictionary<string, string[]> _segmentBuildCodes = new Dictionary<string, string[]>();

        static string _builderDataPath = "../../../../../open-ftth-demo-data/";
        static string _nodeIdPrefix = "0b2168f2-d9be-455c-a4de-e9169f";
        static string _segmentIdPrefix = "b95000fb-425d-4cd3-9f45-66e8c5";

        public static void Run(IMediator CommandBust)
        {
            // Create route nodes and segments
            var graphBuilder = new Wgs84GraphBuilder(_routeGraph);

            ImportRouteNodes(CommandBust, graphBuilder);
            ImportRouteSegments(CommandBust, graphBuilder);
        }

        static void ImportRouteNodes(IMediator CommandBus, Wgs84GraphBuilder graphBuilder)
        {
            // Import node objects to database
            var nodesJsonText = File.ReadAllText(_builderDataPath + "Builder/RouteNodes.geojson");

            var nodesJson = JsonConvert.DeserializeObject(nodesJsonText) as JObject;

            var features = nodesJson["features"];

            foreach (var feature in features)
            {
                var properties = feature["properties"] as JObject;

                var geometry = feature["geometry"];

                var geometryType = geometry["type"].ToString();
                var geometryCoordinates = geometry["coordinates"].ToString().Replace("\r\n", "").Replace(" ", "");

                var nodeId = _nodeIdPrefix + properties["Id"].ToString().PadLeft(6, '0');
                var nodeType = properties["NodeType"].ToString();
                var nodeName = properties["NodeName"].ToString();
                var assetStatus = properties["Status"].ToString();

                if (properties["BuildTestData"].ToString() != "")
                {
                    var buildCodes = properties["BuildTestData"].ToString().Split(';');
                    _nodeBuildCodes.Add(nodeId, buildCodes);
                }


                // Add node to graph
                var x = ((JArray)geometry["coordinates"])[0];
                var y = ((JArray)geometry["coordinates"])[1];
                graphBuilder.AddNodeToGraph(nodeId, (double)x, (double)y);

                // Derive node and function kind
                var nodeKind = RouteNodeKindEnum.Unknown;
                var nodeFunctionKind =RouteNodeFunctionKindEnum.Unknown;

                if (nodeType == "CO")
                {
                    nodeKind = RouteNodeKindEnum.CentralOfficeSmall;
                    nodeFunctionKind = RouteNodeFunctionKindEnum.SecondaryNode;
                }
                else if (nodeType == "HH")
                {
                    nodeKind = RouteNodeKindEnum.HandHole;
                    nodeFunctionKind = RouteNodeFunctionKindEnum.OpenConduitPoint;
                }
                else if (nodeType == "CC")
                {
                    nodeKind = RouteNodeKindEnum.ConduitClosure;
                    nodeFunctionKind = RouteNodeFunctionKindEnum.BurriedConduitPont;
                }
                else if (nodeType == "CE")
                {
                    nodeKind = RouteNodeKindEnum.ConduitEnd;
                    nodeFunctionKind = RouteNodeFunctionKindEnum.BurriedConduitPont;
                }
                else if (nodeType == "SJ")
                {
                    nodeKind = RouteNodeKindEnum.ConduitSimpleJunction;
                    nodeFunctionKind = RouteNodeFunctionKindEnum.BurriedConduitPont;
                }
                else if (nodeType == "FP")
                {
                    nodeKind = RouteNodeKindEnum.CabinetBig;
                    nodeFunctionKind = RouteNodeFunctionKindEnum.FlexPoint;
                }
                else if (nodeType == "SP")
                {
                    nodeKind = RouteNodeKindEnum.CabinetSmall;
                    nodeFunctionKind = RouteNodeFunctionKindEnum.SplicePoint;
                }
                else if (nodeType == "A")
                {
                    nodeKind = RouteNodeKindEnum.BuildingAccessPoint;
                    nodeFunctionKind = RouteNodeFunctionKindEnum.SplicePoint;
                }
                else if (nodeType == "MDU")
                {
                    nodeKind = RouteNodeKindEnum.MultiDwellingUnit;
                    nodeFunctionKind = RouteNodeFunctionKindEnum.ServiceDeliveryPoint;
                }
                else if (nodeType == "SDU")
                {
                    nodeKind = RouteNodeKindEnum.SingleDwellingUnit;
                    nodeFunctionKind = RouteNodeFunctionKindEnum.ServiceDeliveryPoint;
                }

                // location info
                var locationInfo = new LocationInfo();
                locationInfo.Id = Guid.NewGuid();
                locationInfo.AccessAddress = new AccessAddressInfo()
                {
                    MunicipalCode = "0630",
                    MunicipalRoadCode = "1521",
                    StreetName = properties["StreetName"].ToString(),
                    HouseNumber = properties["HouseNumber"].ToString(),
                    PostalCode = "7120",
                    PostalName = "Vejle Ø"
                };


                var addNodeCmd = new AddNodeCommand()
                {
                    Id = Guid.Parse(nodeId),
                    Name = nodeName,
                    NodeKind = nodeKind,
                    NodeFunctionKind = nodeFunctionKind,
                    LocationInfo = locationInfo,
                    Geometry = new Geometry(geometryType, geometryCoordinates)
                };

                CommandBus.Send(addNodeCmd).Wait();
            }
        }

        
        static void ImportRouteSegments(IMediator CommandBus, Wgs84GraphBuilder graphBuilder)
        {
            // Import node objects to database
            var segmentJsonText = File.ReadAllText(_builderDataPath + "Builder/RouteSegments.geojson");

            var segmentsJson = JsonConvert.DeserializeObject(segmentJsonText) as JObject;

            var features = segmentsJson["features"];

            foreach (var feature in features)
            {
                var properties = feature["properties"] as JObject;

                var geometry = feature["geometry"];

                var geometryType = geometry["type"].ToString();
                var geometryCoordinates = geometry["coordinates"].ToString().Replace("\r\n", "").Replace(" ", "");

                var segmentId = _segmentIdPrefix + properties["Id"].ToString().PadLeft(6, '0');
                var segmentKind = properties["RouteSegmentKind"].ToString();
                var assetStatus = properties["Status"].ToString();

                if (properties["BuildTestData"].ToString() != "")
                {
                    var buildCodes = properties["BuildTestData"].ToString().Split(';');
                    _segmentBuildCodes.Add(segmentId, buildCodes);
                }


                // Add link to graph
                var coordinates = geometry["coordinates"] as JArray;
                var startX = coordinates.First[0];
                var startY = coordinates.First[1];

                var endX = coordinates.Last[0];
                var endY = coordinates.Last[1];

                graphBuilder.AddLinkToGraph(segmentId, (double)startX, (double)startY, (double)endX, (double)endY);


                // Derive node and function kind
                var segmentKindCode = RouteSegmentKindEnum.Unknown;

                if (segmentKind == "buried")
                {
                    segmentKindCode = RouteSegmentKindEnum.Underground;
                }

                var link = _routeGraph.Links[segmentId];

                var addSegmentCmd = new AddSegmentCommand()
                {
                    Id = Guid.Parse(segmentId),
                    FromNodeId = Guid.Parse(link.StartNode.Id),
                    ToNodeId = Guid.Parse(link.EndNode.Id),
                    SegmentKind = segmentKindCode,
                    Geometry = new Geometry(geometryType, geometryCoordinates)
                };

                CommandBus.Send(addSegmentCmd).Wait();
            }
        }
        
    }
}
