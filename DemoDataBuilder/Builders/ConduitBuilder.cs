using MemoryGraph;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using QueryModel.Route;

namespace DemoDataBuilder.Builders
{
    public static class ConduitBuilder
    {
        public static void Run(MigrationBuilder migrationBuilder)
        {
            Dictionary<string, ConduitBuildInfo> conduitBuildInfos = new Dictionary<string, ConduitBuildInfo>();

            // First find all main conduits. 
            // Format ConduitSpecCode-ConduitNumber-(Marking) - i.e. "G10F-1-BL"
            // Having no underscores (which are used on single conduits that must connected to multi conduits)
            foreach (var segmentBuildCode in RouteNetworkBuilder.SegmentBuildCodes)
            {
                var segmentId = segmentBuildCode.Key;
                var buildCodes = segmentBuildCode.Value;
                
                foreach (var buildCode in buildCodes)
                {
                    // Ignore build codes with underscore, because they are single conduits that is to be connected to multi conduits that we deal with later
                    if (!buildCode.Contains("_"))
                    {
                        // Extract the different values from build code string
                        string[] buildCodeSplit = buildCode.Split('-');
                        var specCode = buildCodeSplit[0];
                        var conduitNumber = Int32.Parse(buildCodeSplit[1]);
                        string marking = null;
                        if (buildCodeSplit.Length > 2)
                            marking = buildCodeSplit[2];

                        // Create build conduit info object, if not exists
                        if (!conduitBuildInfos.ContainsKey(buildCode))
                        {
                            conduitBuildInfos.Add(buildCode, new ConduitBuildInfo(conduitNumber, specCode, marking));
                        }

                        var conduitBuildInfo = conduitBuildInfos[buildCode];

                        // Add segment id to build info object
                        conduitBuildInfo.AddRelatedRouteSegmentIdBuildInfo(segmentId);
                    }
                }
            }

            // Find breakouts
            foreach (var segmentBuildCode in RouteNetworkBuilder.SegmentBuildCodes)
            {
                var segmentId = segmentBuildCode.Key;
                var buildCodes = segmentBuildCode.Value;

                foreach (var buildCode in buildCodes)
                {
                    // Ignore build codes with underscore, because they are single conduits that is to be connected to multi conduits that we deal with later
                    if (buildCode.Contains("_"))
                    {
                        // Extract the different values from build code string
                        string[] buildCodeSplit = buildCode.Split('_');

                        // Try found main conduit to breakout
                        var conduitBuildInfo = conduitBuildInfos[buildCodeSplit[0]];

                    }
                }
            }



            // Sort and validate all conduit build infos
            foreach (var buildInfo in conduitBuildInfos)
            {
                /*
                buildInfo.Value.SortAndCheckSegmentRelations(RouteNetworkBuilder.RouteGraph);

                var nodeLinkIds = RouteNetworkBuilder.RouteGraph.GetNodeLinkPathFromLinkPath(buildInfo.Value.relatedRouteSegmentIds);

                Guid walkOfInterestId = Guid.NewGuid();

                int seqNo = 1;

                RouteWalkRelationTypeEnum type = RouteWalkRelationTypeEnum.StartNode;

                // Create conduit walk of interests objects
                foreach (var networkElementId in nodeLinkIds)
                {
                    migrationBuilder.InsertData(
                        table: "RouteElementWalkOfInterestRelations",
                        columns: new[] { "RouteElementId", "WalkOfInterestId", "SeqNo", "Type" },
                        values: new object[] {
                            networkElementId,
                            walkOfInterestId,
                            seqNo,
                            (int)type
                        }
                    );

                    if (type == RouteWalkRelationTypeEnum.IntermediateSegment && (seqNo == (nodeLinkIds.Count - 1)))
                        type = RouteWalkRelationTypeEnum.EndNode;
                    else if (type == RouteWalkRelationTypeEnum.IntermediateSegment)
                        type = RouteWalkRelationTypeEnum.IntermediateNode;
                    else if (type == RouteWalkRelationTypeEnum.StartNode || type == RouteWalkRelationTypeEnum.IntermediateNode)
                        type = RouteWalkRelationTypeEnum.IntermediateSegment;
                    else if (type == RouteWalkRelationTypeEnum.EndNode)
                    {
                        // do nothing we're finish
                    }
                    else
                    {
                        throw new NotSupportedException("Something went wrong in route walk relation write logic. Code broken!");
                    }

                    seqNo++;
                    
                }
                */
            }


        }
    }

    public class ConduitBuildInfo
    {
        public Guid Id { get; set; }
        public int BuildCodeNumber { get; set; }
        public string BuildCodeSpecificationCode { get; set; }
        public string BuildCodeMarking { get; set; }

        private List<string> relatedRouteSegmentIds = new List<string>();

        private List<ConduitSegmentBuildInfo> conduitSegments = new List<ConduitSegmentBuildInfo>();

        // List of segments that creates breakout on this conduit
        private List<string> breakoutSegments = new List<string>();

        public ConduitBuildInfo(int number, string specCode, string marking)
        {
            Id = Guid.NewGuid();
            BuildCodeNumber = number;
            BuildCodeSpecificationCode = specCode;
            BuildCodeMarking = marking;
        }

        public void AddRelatedRouteSegmentIdBuildInfo(string relatedRouteSegmentId)
        {
            relatedRouteSegmentIds.Add(relatedRouteSegmentId);
        }

        public void AddBranchOutBuildInfo(string routeSegmentIdThatBreaks)
        {
            breakoutSegments.Add(routeSegmentIdThatBreaks);
        }

        private void SortAndCheckSegmentRelations(Graph routeGraph)
        {
            relatedRouteSegmentIds = routeGraph.SortLinkPath(relatedRouteSegmentIds);
        }
    }

    public class ConduitSegmentBuildInfo
    {
        public int SeqNo { get; set; }

        public List<string> segmentIds = new List<string>();

        public ConduitSegmentBuildInfo(int seqNo)
        {
            SeqNo = seqNo;
        }
    }

    public class ConduitBreakoutBuildInfo
    {
        // Id of the node that breakouts the conduit
        private string breakoutRouteNodeId;

        /// <summary>
        /// Specifices (using the connectivity build code) how a multi conduit should be breaked out and connected to another single or multi conduit.
        /// </summary>
        /// <param name="routeNodeId">Where the breakout should take place</param>
        /// <param name="breakoutConduitBuildId">The multi conduit running through the node, that must be breaked out</param>
        /// <param name="toBeConnectedConduitBuildId">The conduit that end/starts in the node and must be connected to the breakout conduit</param>
        /// <param name="connectivityBuildCode">
        /// If a single number, then the to-be-connected conduit must be a single conduit, and it will be connected to the specified inner conduit breaked out from the breakout conduit.
        /// If 1-1,2-2 format, then it maps inner conduits of the breakout conduit to inner conduits of a to-be-connected multi conduit.
        /// </param>
        public ConduitBreakoutBuildInfo(string routeNodeId, string breakoutConduitBuildId, string toBeConnectedConduitBuildId, string connectivityBuildCode)
        {
            breakoutRouteNodeId = routeNodeId;
        }
    }

    public class ConduitConnectivityNodeBuildInfo
    {
        // Id of the segment that breakouts the conduit
        public string BreakoutSegmentId;

        public ConduitConnectivityNodeBuildInfo()
        {
        }
    }
}
