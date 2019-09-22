using MemoryGraph;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QueryModel.Route;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DemoDataBuilder.Builders
{
    public static class RouteNetworkBuilder
    {
        public static Graph RouteGraph = new Graph();
        public static Dictionary<string, string[]> NodeBuildCodes = new Dictionary<string, string[]>();
        public static Dictionary<string, string[]> SegmentBuildCodes = new Dictionary<string, string[]>();

        static string _builderDataPath = "../../../../../open-ftth-demo-data/";
        static string _nodeIdPrefix = "0b2168f2-d9be-455c-a4de-e9169f";
        static string _segmentIdPrefix = "b95000fb-425d-4cd3-9f45-66e8c5";

        public static void Run(MigrationBuilder migrationBuilder)
        {
            // Create route nodes and segments
            var graphBuilder = new Wgs84GraphBuilder(RouteGraph);

            ImportRouteNodes(migrationBuilder, graphBuilder);
            ImportRouteSegments(migrationBuilder, graphBuilder);
        }

        static void ImportRouteNodes(MigrationBuilder migrationBuilder, Wgs84GraphBuilder graphBuilder)
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
                    NodeBuildCodes.Add(nodeId, buildCodes);
                }


                // Add node to graph
                var x = ((JArray)geometry["coordinates"])[0];
                var y = ((JArray)geometry["coordinates"])[1];
                graphBuilder.AddNodeToGraph(nodeId, (double)x, (double)y);

                // Derive node and function kind
                int nodeKind = (int)RouteNodeKindEnum.Unknown;
                int nodeFunctionKind = (int)RouteNodeFunctionKindEnum.Unknown;

                if (nodeType == "CO")
                {
                    nodeKind = (int)RouteNodeKindEnum.CentralOfficeSmall;
                    nodeFunctionKind = (int)RouteNodeFunctionKindEnum.SecondaryNode;
                }
                else if (nodeType == "HH")
                {
                    nodeKind = (int)RouteNodeKindEnum.HandHole;
                    nodeFunctionKind = (int)RouteNodeFunctionKindEnum.OpenConduitPoint;
                }
                else if (nodeType == "CC")
                {
                    nodeKind = (int)RouteNodeKindEnum.ConduitClosure;
                    nodeFunctionKind = (int)RouteNodeFunctionKindEnum.BurriedConduitPont;
                }
                else if (nodeType == "CE")
                {
                    nodeKind = (int)RouteNodeKindEnum.ConduitEnd;
                    nodeFunctionKind = (int)RouteNodeFunctionKindEnum.BurriedConduitPont;
                }
                else if (nodeType == "SJ")
                {
                    nodeKind = (int)RouteNodeKindEnum.ConduitSimpleJunction;
                    nodeFunctionKind = (int)RouteNodeFunctionKindEnum.BurriedConduitPont;
                }
                else if (nodeType == "FP")
                {
                    nodeKind = (int)RouteNodeKindEnum.CabinetBig;
                    nodeFunctionKind = (int)RouteNodeFunctionKindEnum.FlexPoint;
                }
                else if (nodeType == "SP")
                {
                    nodeKind = (int)RouteNodeKindEnum.CabinetSmall;
                    nodeFunctionKind = (int)RouteNodeFunctionKindEnum.SplicePoint;
                }
                else if (nodeType == "A")
                {
                    nodeKind = (int)RouteNodeKindEnum.BuildingAccessPoint;
                    nodeFunctionKind = (int)RouteNodeFunctionKindEnum.SplicePoint;
                }
                else if (nodeType == "MDU")
                {
                    nodeKind = (int)RouteNodeKindEnum.MultiDwellingUnit;
                    nodeFunctionKind = (int)RouteNodeFunctionKindEnum.ServiceDeliveryPoint;
                }
                else if (nodeType == "SDU")
                {
                    nodeKind = (int)RouteNodeKindEnum.SingleDwellingUnit;
                    nodeFunctionKind = (int)RouteNodeFunctionKindEnum.ServiceDeliveryPoint;
                }


                migrationBuilder.InsertData(
                table: "RouteNodes",
                columns: new[] { "Id", "Name", "NodeKind", "NodeFunctionKind", "GeoJsonType", "GeoJsonCoordinates", "AssetStatus" },
                values: new object[] {
                    nodeId,
                    nodeName,
                    nodeKind,
                    nodeFunctionKind,
                    geometryType,
                    geometryCoordinates,
                    Int32.Parse(assetStatus)
                });

            }
        }

        static void ImportRouteSegments(MigrationBuilder migrationBuilder, Wgs84GraphBuilder graphBuilder)
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
                    SegmentBuildCodes.Add(segmentId, buildCodes);
                }


                // Add link to graph
                var coordinates = geometry["coordinates"] as JArray;
                var startX = coordinates.First[0];
                var startY = coordinates.First[1];

                var endX = coordinates.Last[0];
                var endY = coordinates.Last[1];

                graphBuilder.AddLinkToGraph(segmentId, (double)startX, (double)startY, (double)endX, (double)endY);


                // Derive node and function kind
                int segmentKindCode = (int)RouteSegmentKindEnum.Unknown;

                if (segmentKind == "buried")
                {
                    segmentKindCode = (int)RouteSegmentKindEnum.Underground;
                }

                migrationBuilder.InsertData(
                table: "RouteSegments",
                columns: new[] { "Id", "SegmentKind", "GeoJsonType", "GeoJsonCoordinates", "AssetStatus" },
                values: new object[] {
                    segmentId,
                    segmentKindCode,
                    geometryType,
                    geometryCoordinates,
                    Int32.Parse(assetStatus)
                });

            }
        }
    }
}
