using Core.GraphSupport.Model;
using Core.ReadModel.Network;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Network.Trace
{
    public class TraversalHelper
    {
        public IRouteNetworkState routeNetworkQueryService;

        public TraversalHelper(IRouteNetworkState routeNetworkQueryService)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
        }

        public ISegmentTraversal CreateTraversalInfoFromSegment(ISegment sourceSegment)
        {
            var result = new SegmentTraversalInfo();

            HashSet<Guid> startNodesFound = new HashSet<Guid>();
            HashSet<Guid> endNodesFound = new HashSet<Guid>();

            Guid startNodeId = Guid.Empty;
            Guid endNodeId = Guid.Empty;
            Guid startSegmentId = Guid.Empty;
            Guid endSegmentId = Guid.Empty;

            List<Guid> allNodeIds = new List<Guid>();
            List<Guid> allSegmentIds = new List<Guid>();

            HashSet<ILine> alreadyChecked = new HashSet<ILine>();

            // Get all segments related to the source segment
            var traceResult = sourceSegment.UndirectionalDFS<GraphElement, GraphElement>();

            // Pull out the segments from the trace result
            var segments = traceResult.Where(t => t is ISegment).Select(t => t as ISegment);

            foreach (var segment in segments)
            {
                var rootConduit = segment.Line.GetRoot();

                if (!alreadyChecked.Contains(rootConduit))
                {
                    alreadyChecked.Add(rootConduit);

                    var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(rootConduit.WalkOfInterestId).SubWalk2(segment.FromRouteNodeId, segment.ToRouteNodeId);

                    // add node ids
                    foreach (var nodeId in walkOfInterest.AllNodeIds)
                    {
                        if (!allNodeIds.Contains(nodeId))
                            allNodeIds.Add(nodeId);
                    }

                    // add segment ids
                    foreach (var segmentId in walkOfInterest.AllSegmentIds)
                    {
                        if (!allSegmentIds.Contains(segmentId))
                            allSegmentIds.Add(segmentId);
                    }


                    if (!startNodesFound.Contains(walkOfInterest.StartNodeId))
                    {
                        startNodesFound.Add(walkOfInterest.StartNodeId);
                        startNodesFound.Add(walkOfInterest.EndNodeId);
                        startNodeId = walkOfInterest.StartNodeId;
                        startSegmentId = walkOfInterest.StartSegmentId;
                    }

                    if (!endNodesFound.Contains(walkOfInterest.EndNodeId))
                    {
                        endNodesFound.Add(walkOfInterest.StartNodeId);
                        endNodesFound.Add(walkOfInterest.EndNodeId);
                        endNodeId = walkOfInterest.EndNodeId;
                        endSegmentId = walkOfInterest.EndSegmentId;
                    }
                }
            }

            result.StartRouteNode = routeNetworkQueryService.GetRouteNodeInfo(startNodeId);
            result.EndRouteNode = routeNetworkQueryService.GetRouteNodeInfo(endNodeId);
            result.StartRouteSegment = routeNetworkQueryService.GetRouteSegmentInfo(startSegmentId);
            result.EndRouteSegment = routeNetworkQueryService.GetRouteSegmentInfo(endSegmentId);

            result.AllRouteNodes = new List<INode>();
            foreach (var nodeId in allNodeIds)
                result.AllRouteNodes.Add(routeNetworkQueryService.GetRouteNodeInfo(nodeId));

            result.AllRouteSegments = new List<ISegment>();
            foreach (var segmentId in allSegmentIds)
                result.AllRouteSegments.Add(routeNetworkQueryService.GetRouteSegmentInfo(segmentId));

            result.AllSegments = segments.ToList();

            return result;
        }
    }
}
