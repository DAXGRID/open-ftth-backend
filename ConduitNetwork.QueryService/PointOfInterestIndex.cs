using ConduitNetwork.Events.Model;
using ConduitNetwork.ReadModel;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.QueryService
{
    public class PointOfInterestIndex
    {
        private IRouteNetworkQueryService routeNetworkQueryService;

        private Dictionary<Guid, List<SingleConduitInfo>> _singleConduitByPointOfInterestId = new Dictionary<Guid, List<SingleConduitInfo>>();

        private Dictionary<Guid, List<ConduitSegmentInfo>> _conduitSegmentEndsByPointOfInterestId = new Dictionary<Guid, List<ConduitSegmentInfo>>();

        private Dictionary<Guid, List<ConduitSegmentInfo>> _conduitSegmentPassByPointOfInterestId = new Dictionary<Guid, List<ConduitSegmentInfo>>();

        private Dictionary<Guid, List<ConduitSegmentInfo>> _conduitSegmentPassByRouteSegmenttId = new Dictionary<Guid, List<ConduitSegmentInfo>>();

        public PointOfInterestIndex(IRouteNetworkQueryService routeNetworkQueryService)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
        }

        public List<ConduitSegmentInfo> GetConduitSegmentsThatEndsInRouteNode(Guid pointOfInterestId)
        {
            if (_conduitSegmentEndsByPointOfInterestId.ContainsKey(pointOfInterestId))
                return _conduitSegmentEndsByPointOfInterestId[pointOfInterestId];
            else
                return new List<ConduitSegmentInfo>();
        }

        public List<ConduitSegmentInfo> GetConduitSegmentsThatPassedByRouteNode(Guid pointOfInterestId)
        {
            if (_conduitSegmentPassByPointOfInterestId.ContainsKey(pointOfInterestId))
                return _conduitSegmentPassByPointOfInterestId[pointOfInterestId];
            else
                return new List<ConduitSegmentInfo>();
        }

        public List<ConduitSegmentInfo> GetConduitSegmentsThatPassedByRouteSegment(Guid routeSegmentId)
        {
            if (_conduitSegmentPassByRouteSegmenttId.ContainsKey(routeSegmentId))
                return _conduitSegmentPassByRouteSegmenttId[routeSegmentId];
            else
                return new List<ConduitSegmentInfo>();
        }

        public void Update(ConduitInfo oldConduitInfo, ConduitInfo newConduitInfo)
        {
            UpdateSegmentEndIndex(oldConduitInfo, newConduitInfo);
            UpdateSegmentPassByIndex(oldConduitInfo, newConduitInfo);
            UpdateRouteSegmentPassByIndex(oldConduitInfo, newConduitInfo);
        }

        private void UpdateSegmentEndIndex(ConduitInfo oldConduitInfo, ConduitInfo newConduitInfo)
        {
            if (oldConduitInfo != null)
            {
                // Remove all old references
                foreach (var segment in oldConduitInfo.Segments)
                {
                    _conduitSegmentEndsByPointOfInterestId[segment.FromNodeId].Remove(segment);
                    _conduitSegmentEndsByPointOfInterestId[segment.ToNodeId].Remove(segment);
                }
            }

            // Add new references
            foreach (var segment in newConduitInfo.Segments)
            {
                if (!_conduitSegmentEndsByPointOfInterestId.ContainsKey(segment.FromNodeId))
                    _conduitSegmentEndsByPointOfInterestId[segment.FromNodeId] = new List<ConduitSegmentInfo>();

                _conduitSegmentEndsByPointOfInterestId[segment.FromNodeId].Add(segment);

                if (!_conduitSegmentEndsByPointOfInterestId.ContainsKey(segment.ToNodeId))
                    _conduitSegmentEndsByPointOfInterestId[segment.ToNodeId] = new List<ConduitSegmentInfo>();

                _conduitSegmentEndsByPointOfInterestId[segment.ToNodeId].Add(segment);
            }
        }

        private void UpdateSegmentPassByIndex(ConduitInfo oldConduitInfo, ConduitInfo newConduitInfo)
        {
            var conduitWalkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(newConduitInfo.GetRootConduit().WalkOfInterestId);

            if (oldConduitInfo != null)
            {
                // Remove old references
                foreach (var segment in oldConduitInfo.Segments)
                {
                    var passThrougNodes = GetPassThroughNodes(segment, conduitWalkOfInterest);

                    foreach (var passThroughNode in passThrougNodes)
                        _conduitSegmentPassByPointOfInterestId[passThroughNode].Remove(segment);
                }
            }

            // Add new references
            foreach (var segment in newConduitInfo.Segments)
            {
                var passThrougNodes = GetPassThroughNodes(segment, conduitWalkOfInterest);

                foreach (var passThroughNode in passThrougNodes)
                {
                    if (!_conduitSegmentPassByPointOfInterestId.ContainsKey(passThroughNode))
                        _conduitSegmentPassByPointOfInterestId[passThroughNode] = new List<ConduitSegmentInfo>();

                    _conduitSegmentPassByPointOfInterestId[passThroughNode].Add(segment);
                }
            }
        }

        private void UpdateRouteSegmentPassByIndex(ConduitInfo oldConduitInfo, ConduitInfo newConduitInfo)
        {
            var conduitWalkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(newConduitInfo.GetRootConduit().WalkOfInterestId);

            if (oldConduitInfo != null)
            {
                // Remove old references
                foreach (var segment in oldConduitInfo.Segments)
                {
                    var passThroughRouteSegments = GetPassThroughRouteSegments(segment, conduitWalkOfInterest);

                    foreach (var passThroughRouteSegment in passThroughRouteSegments)
                        _conduitSegmentPassByRouteSegmenttId[passThroughRouteSegment].Remove(segment);
                }
            }

            // Add new references
            foreach (var segment in newConduitInfo.Segments)
            {
                var passThroughRouteSegments = GetPassThroughRouteSegments(segment, conduitWalkOfInterest);

                foreach (var passThroughRouteSegment in passThroughRouteSegments)
                {
                    if (!_conduitSegmentPassByRouteSegmenttId.ContainsKey(passThroughRouteSegment))
                        _conduitSegmentPassByRouteSegmenttId[passThroughRouteSegment] = new List<ConduitSegmentInfo>();

                    _conduitSegmentPassByRouteSegmenttId[passThroughRouteSegment].Add(segment);
                }
            }
        }


        private List<Guid> GetPassThroughNodes(ConduitSegmentInfo conduitSegment, WalkOfInterestInfo conduitWalkOfInterest)
        {
            List<Guid> result = new List<Guid>();

            // Get pass through node candidates (the ones that the segment don't start or end at)
            HashSet<Guid> passThroughNodeCandidatess = new HashSet<Guid>();

            foreach (var nodeId in conduitWalkOfInterest.AllNodeIds)
            {
                if (!(conduitSegment.FromNodeId == nodeId || conduitSegment.ToNodeId == nodeId))
                {
                    passThroughNodeCandidatess.Add(nodeId);
                }
            }

            var segmentWalkNodes = conduitWalkOfInterest.SubWalk2(conduitSegment.FromNodeId, conduitSegment.ToNodeId).AllNodeIds;

            foreach (var segmentWalkNode in segmentWalkNodes)
            {
                if (passThroughNodeCandidatess.Contains(segmentWalkNode))
                    result.Add(segmentWalkNode);
            }

            return result;
        }

        private List<Guid> GetPassThroughRouteSegments(ConduitSegmentInfo conduitSegment, WalkOfInterestInfo conduitWalkOfInterest)
        {
            List<Guid> result = new List<Guid>();

            // Get pass through segment candidates (the ones that the segment don't start or end at)
            HashSet<Guid> passThroughSegmentCandidatess = new HashSet<Guid>();

            foreach (var segmentId in conduitWalkOfInterest.AllSegmentIds)
            {
                passThroughSegmentCandidatess.Add(segmentId);
            }

            var segmentWalkSegments = conduitWalkOfInterest.SubWalk2(conduitSegment.FromNodeId, conduitSegment.ToNodeId).AllSegmentIds;

            foreach (var segmentWalk in segmentWalkSegments)
            {
                if (passThroughSegmentCandidatess.Contains(segmentWalk))
                    result.Add(segmentWalk);
            }

            return result;
        }
    }
}