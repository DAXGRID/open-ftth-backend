using Microsoft.Extensions.DependencyInjection;
using MemoryGraph;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Demo.BuildTestNetwork.Builders;
using Demo.BuildTestNetwork;
using RouteNetwork.Business.Commands;
using ConduitNetwork.Business.Commands;
using ConduitNetwork.Events.Model;
using RouteNetwork.QueryService;
using ConduitNetwork.QueryService;
using MediatR;

namespace Demo.BuildTestNetwork.Builders
{
    public static class ConduitBuilder
    {
        public static void Run(IConduitNetworkQueryService conduitNetworkQueryService, IMediator CommandBus)
        {
            Dictionary<string, ConduitBuildInfo> conduitBuildInfos = new Dictionary<string, ConduitBuildInfo>();


            // First find all main conduits. 
            // Format ConduitSpecCode-ConduitNumber-(Marking) - i.e. "G10F-1-BL"
            // Having no underscores (which are used on single conduits that must connected to multi conduits)
            foreach (var segmentBuildCode in RouteNetworkBuilder._segmentBuildCodes)
            {
                var segmentId = segmentBuildCode.Key;
                var buildCodes = segmentBuildCode.Value;
                
                foreach (var buildCode in buildCodes)
                {
                    
                    // Single conduits (that is connected to a multi conduit) have "_" in spec code
                    if (buildCode.Contains("_"))
                    {
                        var newConduitBuildInfo = new ConduitBuildInfo();
                        newConduitBuildInfo.BuildCode = buildCode;
                        newConduitBuildInfo.MultiConduit = false;
                        newConduitBuildInfo.BuildCodeSpecificationCode = "Ø12";

                        // Create build conduit info object, if not exists
                        if (!conduitBuildInfos.ContainsKey(buildCode))
                        {
                            conduitBuildInfos.Add(buildCode, newConduitBuildInfo);
                        }

                        var conduitBuildInfo = conduitBuildInfos[buildCode];

                        // Add segment id to build info object
                        conduitBuildInfo.AddRelatedRouteSegmentIdBuildInfo(segmentId);
                    }
                    else
                    // We're delaing with a multi conduit or single conduit not connected 
                    {
                        var newConduitBuildInfo = new ConduitBuildInfo();
                        newConduitBuildInfo.BuildCode = buildCode;
                        string[] buildCodeSplit = buildCode.Split('-');
                        newConduitBuildInfo.BuildCodeSpecificationCode = buildCodeSplit[0];
                        newConduitBuildInfo.BuildCodeConduitId = Int32.Parse(buildCodeSplit[1]);

                        if (buildCode.StartsWith("S"))
                            newConduitBuildInfo.MultiConduit = false;
                        else
                            newConduitBuildInfo.MultiConduit = true;

                        if (buildCodeSplit.Length > 2)
                            newConduitBuildInfo.BuildCodeMarking = buildCodeSplit[2];

                        // Create build conduit info object, if not exists
                        if (!conduitBuildInfos.ContainsKey(buildCode))
                        {
                            conduitBuildInfos.Add(buildCode, newConduitBuildInfo);
                        }

                        var conduitBuildInfo = conduitBuildInfos[buildCode];

                        // Add segment id to build info object
                        conduitBuildInfo.AddRelatedRouteSegmentIdBuildInfo(segmentId);
                    }
                }
            }



            // Sort and validate all conduit build infos
            foreach (var conduitBuildInfo in conduitBuildInfos)
            {
                conduitBuildInfo.Value.Prepare(RouteNetworkBuilder._routeGraph);

                var registerWalkOfInterestCmd = new RegisterWalkOfInterestCommand();
                registerWalkOfInterestCmd.WalkOfInterestId = conduitBuildInfo.Value.WalkOfInterestId;

                // First create walk of interest
                registerWalkOfInterestCmd.RouteElementIds = RouteNetworkBuilder._routeGraph.GetNodeLinkPathFromLinkPath(conduitBuildInfo.Value.relatedRouteSegmentIds);
                CommandBus.Send(registerWalkOfInterestCmd).Wait();

                // Create multi conduit
                if (conduitBuildInfo.Value.MultiConduit)
                {
                    var placeMultiConduitCommand = new PlaceMultiConduitCommand()
                    {
                        MultiConduitId = conduitBuildInfo.Value.Id,
                        WalkOfInterestId = conduitBuildInfo.Value.WalkOfInterestId,
                        DemoDataSpec = conduitBuildInfo.Key
                    };

                    CommandBus.Send(placeMultiConduitCommand).Wait();
                }
                // Create single conduit
                else
                {
                    var placeSingleConduitCommand = new PlaceSingleConduitCommand()
                    {
                        SingleConduitId = conduitBuildInfo.Value.Id,
                        WalkOfInterestId = conduitBuildInfo.Value.WalkOfInterestId,
                        DemoDataSpec = conduitBuildInfo.Key
                    };

                    CommandBus.Send(placeSingleConduitCommand).Wait();
                }

            }

            // Do the cuts and connections

            HashSet<string> cutAlreadDone = new HashSet<string>();

            foreach (var segmentBuildCode in RouteNetworkBuilder._segmentBuildCodes)
            {
                var segmentId = segmentBuildCode.Key;
                var buildCodes = segmentBuildCode.Value;

                foreach (var buildCode in buildCodes)
                {
                    // Build codes containing _, is where conduits should be connected to each other
                    if (buildCode.Contains("_") && !cutAlreadDone.Contains(buildCode))
                    {
                        cutAlreadDone.Add(buildCode);

                        // Extract the different values from build code string
                        string[] buildCodeSplit = buildCode.Split('_');

                        int innerConduitNumberToCut = Int32.Parse(buildCodeSplit[1]);

                        // Find multi conduit to breakout/connect
                        var multiConduitBuildInfo = conduitBuildInfos[buildCodeSplit[0]];
                        var multiConduitWalkOfInterest = RouteNetworkBuilder._routeGraph.GetNodeLinkPathFromLinkPath(multiConduitBuildInfo.relatedRouteSegmentIds);

                        // Find single conduit that has to be connected to the multi conduit
                        var singleConduitBuildInfo = conduitBuildInfos[buildCode];
                        var singleConduitSegments = RouteNetworkBuilder._segmentBuildCodes.Where(n => n.Value.Contains(buildCode)).ToList();

                        // Find segment that har start or end in multi conduit
                        Link singleConduitLink = null;

                        foreach (var singleConduitSegmentId in singleConduitSegments)
                        {
                            singleConduitLink = RouteNetworkBuilder._routeGraph.Links[singleConduitSegmentId.Key];

                            if (multiConduitWalkOfInterest.Contains(Guid.Parse(singleConduitLink.StartNode.Id)))
                                break;

                            if (multiConduitWalkOfInterest.Contains(Guid.Parse(singleConduitLink.EndNode.Id)))
                                break;
                        }


                        // find the point of interest (the node where the single conduit connected witht the multi conduit)
                        Guid pointOfInterest = Guid.Empty;

                        ConduitEndKindEnum customerConduitConnectKind = ConduitEndKindEnum.Outgoing;

                        if (multiConduitWalkOfInterest.Contains(Guid.Parse(singleConduitLink.StartNode.Id)))
                            pointOfInterest = Guid.Parse(singleConduitLink.StartNode.Id);
                        else if (multiConduitWalkOfInterest.Contains(Guid.Parse(singleConduitLink.EndNode.Id)))
                        {
                            pointOfInterest = Guid.Parse(singleConduitLink.EndNode.Id);
                            customerConduitConnectKind = ConduitEndKindEnum.Outgoing;
                        }


                        if (buildCode == "G12F-1-BL_3")
                        {

                        }

                        // Cut the inner conduit in the multi conduit, if not end
                        if (!(multiConduitWalkOfInterest[0] == pointOfInterest || multiConduitWalkOfInterest[multiConduitWalkOfInterest.Count - 1] == pointOfInterest))
                        {
                            
                            // First cut the outer conduit, if not already cut
                            if (!conduitNetworkQueryService.CheckIfConduitIsCut(multiConduitBuildInfo.Id, pointOfInterest))
                            {
                                var cutOuterConduitCommand = new CutOuterConduitCommand()
                                {
                                    MultiConduitId = multiConduitBuildInfo.Id,
                                    PointOfInterestId = pointOfInterest,
                                };

                                CommandBus.Send(cutOuterConduitCommand).Wait();
                            }

                            // Cut the inner conduit
                            var cutInnerConduitCommand = new CutInnerConduitCommand()
                            {
                                MultiConduitId = multiConduitBuildInfo.Id,
                                PointOfInterestId = pointOfInterest,
                                InnerConduitSequenceNumber = innerConduitNumberToCut
                            };

                            CommandBus.Send(cutInnerConduitCommand).Wait();
                        }

                        // Junction
                        Guid newJunctionId = Guid.NewGuid();

                        // Connect inner conduit in the multi conduit
                        var connectInnerConduitCommand = new ConnectInnerConduitToJunction()
                        {
                            MultiConduitId = multiConduitBuildInfo.Id,
                            PointOfInterestId = pointOfInterest,
                            InnerConduitSequenceNumber = innerConduitNumberToCut,
                            ConnectedJunctionId = newJunctionId,
                            ConnectedEndKind = ConduitNetwork.Events.Model.ConduitEndKindEnum.Incomming
                        };

                        CommandBus.Send(connectInnerConduitCommand).Wait();

                        // Connect customer conduit to the multi conduit
                        var connectCustomerConduitCommand = new ConnectSingleConduitToJunction()
                        {
                            SingleConduitId = singleConduitBuildInfo.Id,
                            PointOfInterestId = pointOfInterest,
                            ConnectedJunctionId = newJunctionId,
                            ConnectedEndKind = customerConduitConnectKind
                        };

                        CommandBus.Send(connectCustomerConduitCommand).Wait();

                    }
                }
            }





            /*
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
                */





        }
    }

    public class ConduitBuildInfo
    {
        public Guid Id { get; set; }

        public bool MultiConduit { get; set; }

        public string BuildCode { get; set; }
        public int BuildCodeConduitId { get; set; }
        public string BuildCodeSpecificationCode { get; set; }
        public string BuildCodeMarking { get; set; }

        public Guid WalkOfInterestId { get; set; }

        public List<string> relatedRouteSegmentIds = new List<string>();

        private List<ConduitSegmentBuildInfo> conduitSegments = new List<ConduitSegmentBuildInfo>();

        // List of segments that creates breakout on this conduit
        private List<string> breakoutSegments = new List<string>();

        public ConduitBuildInfo()
        {
            /*
            Id = GUIDHelper.StringToGUID("conduit" + BuildCodeSpecificationCode + BuildCodeConduitId + BuildCodeMarking);
            BuildCodeNumber = number;
            BuildCodeSpecificationCode = specCode;
            BuildCodeMarking = marking;
            */
        }

        public void AddRelatedRouteSegmentIdBuildInfo(string relatedRouteSegmentId)
        {
            relatedRouteSegmentIds.Add(relatedRouteSegmentId);
        }

        public void AddBranchOutBuildInfo(string routeSegmentIdThatBreaks)
        {
            breakoutSegments.Add(routeSegmentIdThatBreaks);
        }

        public void Prepare(Graph routeGraph)
        {
            relatedRouteSegmentIds = routeGraph.SortLinkPath(relatedRouteSegmentIds);

            Id = GUIDHelper.StringToGUID("conduit" + relatedRouteSegmentIds[0].ToString() + BuildCode);
            WalkOfInterestId = GUIDHelper.StringToGUID("woi" + relatedRouteSegmentIds[0].ToString() + BuildCode);
        }

        public override string ToString()
        {
            string result = MultiConduit ? "MultiConduit " : "SingleConduit ";
            result += BuildCodeSpecificationCode + " ";

            return result;
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
