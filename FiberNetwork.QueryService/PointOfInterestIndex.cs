using Core.ReadModel.Network;
using FiberNetwork.Events.Model;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiberNetwork.QueryService
{
    public class PointOfInterestIndex
    {
        private IRouteNetworkState routeNetworkQueryService;

        private Dictionary<Guid, List<ILineSegment>> _fiberSegmentEndsByPointOfInterestId = new Dictionary<Guid, List<ILineSegment>>();

        private Dictionary<Guid, List<ILineSegment>> _fiberSegmentPassByPointOfInterestId = new Dictionary<Guid, List<ILineSegment>>();

        private Dictionary<Guid, List<ILineSegment>> _fiberSegmentPassByRouteSegmenttId = new Dictionary<Guid, List<ILineSegment>>();

        public PointOfInterestIndex(IRouteNetworkState routeNetworkQueryService)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
        }

        public List<FiberSegmentInfo> GetConduitSegmentsThatEndsInRouteNode(Guid pointOfInterestId)
        {
            if (_fiberSegmentEndsByPointOfInterestId.ContainsKey(pointOfInterestId))
                return _fiberSegmentEndsByPointOfInterestId[pointOfInterestId].OfType<FiberSegmentInfo>().ToList();
            else
                return new List<FiberSegmentInfo>();
        }

        public List<FiberSegmentInfo> GetConduitSegmentsThatPassedByRouteNode(Guid pointOfInterestId)
        {
            if (_fiberSegmentPassByPointOfInterestId.ContainsKey(pointOfInterestId))
                return _fiberSegmentPassByPointOfInterestId[pointOfInterestId].OfType<FiberSegmentInfo>().ToList();
            else
                return new List<FiberSegmentInfo>();
        }

        public List<FiberSegmentInfo> GetConduitSegmentsThatPassedByRouteSegment(Guid routeSegmentId)
        {
            if (_fiberSegmentPassByRouteSegmenttId.ContainsKey(routeSegmentId))
                return _fiberSegmentPassByRouteSegmenttId[routeSegmentId].OfType<FiberSegmentInfo>().ToList();
            else
                return new List<FiberSegmentInfo>();
        }

        public void Update(FiberInfo oldConduitInfo, FiberInfo newConduitInfo)
        {
            UpdateSegmentEndIndex(oldConduitInfo, newConduitInfo);
            UpdateSegmentPassByIndex(oldConduitInfo, newConduitInfo);
            UpdateRouteSegmentPassByIndex(oldConduitInfo, newConduitInfo);
        }

        private void UpdateSegmentEndIndex(FiberInfo oldConduitInfo, FiberInfo newConduitInfo)
        {
            if (oldConduitInfo != null)
            {
                // Remove all old references
                foreach (var segment in oldConduitInfo.Segments)
                {
                    _fiberSegmentEndsByPointOfInterestId[segment.FromRouteNodeId].Remove(segment);
                    _fiberSegmentEndsByPointOfInterestId[segment.ToRouteNodeId].Remove(segment);
                }
            }

            // Add new references
            foreach (var segment in newConduitInfo.Segments)
            {
                if (!_fiberSegmentEndsByPointOfInterestId.ContainsKey(segment.FromRouteNodeId))
                    _fiberSegmentEndsByPointOfInterestId[segment.FromRouteNodeId] = new List<ILineSegment>();

                _fiberSegmentEndsByPointOfInterestId[segment.FromRouteNodeId].Add(segment);

                if (!_fiberSegmentEndsByPointOfInterestId.ContainsKey(segment.ToRouteNodeId))
                    _fiberSegmentEndsByPointOfInterestId[segment.ToRouteNodeId] = new List<ILineSegment>();

                _fiberSegmentEndsByPointOfInterestId[segment.ToRouteNodeId].Add(segment);
            }
        }

        private void UpdateSegmentPassByIndex(FiberInfo oldFiberInfo, FiberInfo newFiberInfo)
        {
            var conduitWalkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(newFiberInfo.GetRoot().WalkOfInterestId);

            if (oldFiberInfo != null)
            {
                // Remove old references
                foreach (var segment in oldFiberInfo.Segments.OfType<FiberSegmentInfo>())
                {
                    var passThrougNodes = GetPassThroughNodes(segment, conduitWalkOfInterest);

                    foreach (var passThroughNode in passThrougNodes)
                        _fiberSegmentPassByPointOfInterestId[passThroughNode].Remove(segment);
                }
            }

            // Add new references
            foreach (var segment in newFiberInfo.Segments.OfType<FiberSegmentInfo>())
            {
                var passThrougNodes = GetPassThroughNodes(segment, conduitWalkOfInterest);

                foreach (var passThroughNode in passThrougNodes)
                {
                    if (!_fiberSegmentPassByPointOfInterestId.ContainsKey(passThroughNode))
                        _fiberSegmentPassByPointOfInterestId[passThroughNode] = new List<ILineSegment>();

                    _fiberSegmentPassByPointOfInterestId[passThroughNode].Add(segment);
                }
            }
        }

        private void UpdateRouteSegmentPassByIndex(FiberInfo oldFiberInfo, FiberInfo newFiberInfo)
        {
            var conduitWalkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(newFiberInfo.GetRoot().WalkOfInterestId);

            if (oldFiberInfo != null)
            {
                // Remove old references
                foreach (var segment in oldFiberInfo.Segments.OfType<FiberSegmentInfo>())
                {
                    var passThroughRouteSegments = GetPassThroughRouteSegments(segment, conduitWalkOfInterest);

                    foreach (var passThroughRouteSegment in passThroughRouteSegments)
                        _fiberSegmentPassByRouteSegmenttId[passThroughRouteSegment].Remove(segment);
                }
            }

            // Add new references
            foreach (var segment in newFiberInfo.Segments.OfType<FiberSegmentInfo>())
            {
                var passThroughRouteSegments = GetPassThroughRouteSegments(segment, conduitWalkOfInterest);

                foreach (var passThroughRouteSegment in passThroughRouteSegments)
                {
                    if (!_fiberSegmentPassByRouteSegmenttId.ContainsKey(passThroughRouteSegment))
                        _fiberSegmentPassByRouteSegmenttId[passThroughRouteSegment] = new List<ILineSegment>();

                    _fiberSegmentPassByRouteSegmenttId[passThroughRouteSegment].Add(segment);
                }
            }
        }


        private List<Guid> GetPassThroughNodes(FiberSegmentInfo fiberSegment, WalkOfInterestInfo conduitWalkOfInterest)
        {
            List<Guid> result = new List<Guid>();

            // Get pass through node candidates (the ones that the segment don't start or end at)
            HashSet<Guid> passThroughNodeCandidatess = new HashSet<Guid>();

            foreach (var nodeId in conduitWalkOfInterest.AllNodeIds)
            {
                if (!(fiberSegment.FromRouteNodeId == nodeId || fiberSegment.ToRouteNodeId == nodeId))
                {
                    passThroughNodeCandidatess.Add(nodeId);
                }
            }

            var segmentWalkNodes = conduitWalkOfInterest.SubWalk2(fiberSegment.FromRouteNodeId, fiberSegment.ToRouteNodeId).AllNodeIds;

            foreach (var segmentWalkNode in segmentWalkNodes)
            {
                if (passThroughNodeCandidatess.Contains(segmentWalkNode))
                    result.Add(segmentWalkNode);
            }

            return result;
        }

        private List<Guid> GetPassThroughRouteSegments(FiberSegmentInfo fiberSegment, WalkOfInterestInfo conduitWalkOfInterest)
        {
            List<Guid> result = new List<Guid>();

            // Get pass through segment candidates (the ones that the segment don't start or end at)
            HashSet<Guid> passThroughSegmentCandidatess = new HashSet<Guid>();

            foreach (var segmentId in conduitWalkOfInterest.AllSegmentIds)
            {
                passThroughSegmentCandidatess.Add(segmentId);
            }

            var segmentWalkSegments = conduitWalkOfInterest.SubWalk2(fiberSegment.FromRouteNodeId, fiberSegment.ToRouteNodeId).AllSegmentIds;

            foreach (var segmentWalk in segmentWalkSegments)
            {
                if (passThroughSegmentCandidatess.Contains(segmentWalk))
                    result.Add(segmentWalk);
            }

            return result;
        }
    }
}