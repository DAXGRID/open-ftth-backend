using AutoMapper;
using ConduitNetwork.Events.Model;
using ConduitNetwork.ReadModel;
using Marten;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConduitNetwork.QueryService
{
    public class ConduitNetworkQueryService : IConduitNetworkQueryService
    {
        private IDocumentStore documentStore = null;

        private PointOfInterestIndex _pointOfInterestIndex;

        private IRouteNetworkState routeNetworkQueryService = null;

        private Dictionary<Guid, MultiConduitInfo> _multiConduitInfos = new Dictionary<Guid, MultiConduitInfo>();

        private Dictionary<Guid, SingleConduitInfo> _singleConduitInfos = new Dictionary<Guid, SingleConduitInfo>();

        private Dictionary<Guid, SingleConduitSegmentJunctionInfo> _singleConduitJuncionInfos = new Dictionary<Guid, SingleConduitSegmentJunctionInfo>();

        private IMapper _mapper = null;


        public ConduitNetworkQueryService(IDocumentStore documentStore, IRouteNetworkState routeNetworkQueryService)
        {
            this.documentStore = documentStore;
            this.routeNetworkQueryService = routeNetworkQueryService;
            this._pointOfInterestIndex = new PointOfInterestIndex(routeNetworkQueryService);
          

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SingleConduitInfo, SingleConduitInfo>();
                cfg.CreateMap<MultiConduitInfo, MultiConduitInfo>();
            });


            _mapper = config.CreateMapper();

            Load();

        }

        private void Load()
        {
            _multiConduitInfos = new Dictionary<Guid, MultiConduitInfo>();
            _singleConduitInfos = new Dictionary<Guid, SingleConduitInfo>();
            _singleConduitJuncionInfos = new Dictionary<Guid, SingleConduitSegmentJunctionInfo>();
            _pointOfInterestIndex = new PointOfInterestIndex(routeNetworkQueryService);

            using (var session = documentStore.LightweightSession())
            {
                // Fetch everything into memory for fast access
                var multiConduits = session.Query<MultiConduitInfo>();

                foreach (var multiConduit in multiConduits)
                {
                    UpdateMultiConduitInfo(multiConduit);
                }

                var singleConduits = session.Query<SingleConduitInfo>();

                foreach (var singleConduit in singleConduits)
                {
                    UpdateSingleConduitInfo(singleConduit);
                }
            }
        }

        public bool CheckIfMultiConduitIdExists(Guid id)
        {
            return _multiConduitInfos.ContainsKey(id);
        }

        public bool CheckIfSingleConduitIdExists(Guid id)
        {
            return _singleConduitInfos.ContainsKey(id);
        }

        public bool CheckIfConduitIsCut(Guid conduitId, Guid pointOfInterestId)
        {
            ConduitInfo conduitToCheck = null;

            if (_singleConduitInfos.ContainsKey(conduitId))
                conduitToCheck = _singleConduitInfos[conduitId];
            else if (_multiConduitInfos.ContainsKey(conduitId))
                    conduitToCheck = _multiConduitInfos[conduitId];
            else
            {
                throw new KeyNotFoundException("Cannot find any conduit with id: " + conduitId);
            }

            // Check if conduit is cut
            if (conduitToCheck.Segments.Exists(s => s.FromNodeId == pointOfInterestId || s.ToNodeId == pointOfInterestId))
                return true;
            else
                return false;
        }

        public ConduitInfo GetConduitInfo(Guid id)
        {
            if (_singleConduitInfos.ContainsKey(id))
                return _singleConduitInfos[id];

            if (_multiConduitInfos.ContainsKey(id))
                return _multiConduitInfos[id];


            throw new KeyNotFoundException("Cannot find any conduit with id: " + id);
        }

        public SingleConduitInfo GetSingleConduitInfo(Guid id)
        {
            if (_singleConduitInfos.ContainsKey(id))
                return _singleConduitInfos[id];

           throw new KeyNotFoundException("Cannot find any single conduit info with id: " + id);
        }

        public MultiConduitInfo GetMultiConduitInfo(Guid id)
        {
            if (_multiConduitInfos.ContainsKey(id))
                return _multiConduitInfos[id];

            throw new KeyNotFoundException("Cannot find any multi conduit info with id: " + id);
        }

        public SingleConduitSegmentJunctionInfo GetSingleConduitSegmentJunctionInfo(Guid id)
        {
            if (_singleConduitJuncionInfos.ContainsKey(id))
                return _singleConduitJuncionInfos[id];

            throw new KeyNotFoundException("Cannot find any single conduit segment conduit info with id: " + id);
        }

        public List<ConduitRelationInfo> GetConduitSegmentsRelatedToPointOfInterest(Guid pointOfInterestId, string conduitId = null)
        {
            List<ConduitRelationInfo> result = new List<ConduitRelationInfo>();

            var conduitSegments = _pointOfInterestIndex.GetConduitSegmentsThatEndsInRouteNode(pointOfInterestId);

            foreach (var conduitSegment in conduitSegments)
            {
                // If conduit segment id set, skip until we read conduit segment specified
                if (conduitId != null)
                {
                    var idToCheck = Guid.Parse(conduitId);

                    if (conduitSegment.Id != idToCheck)
                        continue;
                }

                ConduitRelationInfo relInfo = new ConduitRelationInfo();

                if (conduitSegment.ToNodeId == pointOfInterestId)
                    result.Add(new ConduitRelationInfo() { Segment = conduitSegment, Type = ConduitRelationTypeEnum.Incomming });

                if (conduitSegment.FromNodeId == pointOfInterestId)
                    result.Add(new ConduitRelationInfo() { Segment = conduitSegment, Type = ConduitRelationTypeEnum.Outgoing });

            }

            var conduitSegmentPassBy = _pointOfInterestIndex.GetConduitSegmentsThatPassedByRouteNode(pointOfInterestId);

            foreach (var conduitSegment in conduitSegmentPassBy)
            {
                // If conduit segment id set, skip until we read conduit segment specified
                if (conduitId != null)
                {
                    var idToCheck = Guid.Parse(conduitId);

                    if (conduitSegment.Id != idToCheck)
                        continue;
                }

                result.Add(new ConduitRelationInfo() { Segment = conduitSegment, Type = ConduitRelationTypeEnum.PassThrough });
            }

            return result;
        }


        public List<ConduitRelationInfo> GetConduitSegmentsRelatedToRouteSegment(Guid routeSegmentId, string conduitId = null)
        {
            List<ConduitRelationInfo> result = new List<ConduitRelationInfo>();

            var conduitSegments = _pointOfInterestIndex.GetConduitSegmentsThatPassedByRouteSegment(routeSegmentId);

            foreach (var conduitSegment in conduitSegments)
            {
                // If conduit segment id set, skip until we read conduit segment specified
                if (conduitId != null)
                {
                    var idToCheck = Guid.Parse(conduitId);

                    if (conduitSegment.Id != idToCheck)
                        continue;
                }

                result.Add(new ConduitRelationInfo() { Segment = conduitSegment, Type = ConduitRelationTypeEnum.PassThrough });
            }

            return result;
        }



        #region utility functions that can be used to create derived info objects
        public ConduitLineInfo CreateConduitLineInfoFromConduitSegment(ConduitSegmentInfo sourceConduitSegment)
        {
            var result = new ConduitLineInfo();

            HashSet<Guid> startNodesFound = new HashSet<Guid>();
            HashSet<Guid> endNodesFound = new HashSet<Guid>();

            Guid startNodeId = Guid.Empty;
            Guid endNodeId = Guid.Empty;
            Guid startSegmentId = Guid.Empty;
            Guid endSegmentId = Guid.Empty;

            List<Guid> allNodeIds = new List<Guid>();
            List<Guid> allSegmentIds = new List<Guid>();

            HashSet<ConduitInfo> alreadyChecked = new HashSet<ConduitInfo>();

            // Get all segments related to the source segment
            var traceResult = sourceConduitSegment.UndirectionalDFS<ConduitSegmentJunctionInfo, ConduitSegmentInfo>();

            // Pull out the conduit segments from the trace result
            var conduitSegments = traceResult.Where(t => t is ConduitSegmentInfo).Select(t => t as ConduitSegmentInfo);

            foreach (var segment in conduitSegments)
            {
                var rootConduit = segment.Conduit.GetRootConduit();

                if (!alreadyChecked.Contains(rootConduit))
                {
                    alreadyChecked.Add(rootConduit);

                    var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(rootConduit.WalkOfInterestId).SubWalk2(segment.FromNodeId, segment.ToNodeId);

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

            result.AllRouteNodes = new List<RouteNodeInfo>();
            foreach (var nodeId in allNodeIds)
                result.AllRouteNodes.Add(routeNetworkQueryService.GetRouteNodeInfo(nodeId));

            result.AllRouteSegments = new List<RouteSegmentInfo>();
            foreach (var segmentId in allSegmentIds)
                result.AllRouteSegments.Add(routeNetworkQueryService.GetRouteSegmentInfo(segmentId));

            result.AllConduitSegments = conduitSegments.ToList();

            return result;
        }

        #endregion


        #region functions called during projection and snapshot reading

        public void UpdateMultiConduitInfo(MultiConduitInfo multiConduitInfo, bool load = false)
        {
            // Resolve segment references
            ResolveSegmentReferences(multiConduitInfo);

            // Update
            if (_multiConduitInfos.ContainsKey(multiConduitInfo.Id))
            {
                var existingMultiConduitInfo = _multiConduitInfos[multiConduitInfo.Id];

                // Update node to segment dictionary
                _pointOfInterestIndex.Update(existingMultiConduitInfo, multiConduitInfo);

                // Save the children
                multiConduitInfo.Children = new List<ConduitInfo>();
                multiConduitInfo.Children.AddRange(existingMultiConduitInfo.Children);

                _mapper.Map<MultiConduitInfo, MultiConduitInfo>(multiConduitInfo, existingMultiConduitInfo);


            }
            // Insert
            else
            {
                // Update node to segment dictionary
                _pointOfInterestIndex.Update(null, multiConduitInfo);

                _multiConduitInfos.Add(multiConduitInfo.Id, multiConduitInfo);
            }
        }

        public void UpdateSingleConduitInfo(SingleConduitInfo singleConduitInfo)
        {
            #region reference resolving

            // Parent multi conduit
            if (singleConduitInfo.MultiConduitId != Guid.Empty)
                singleConduitInfo.Parent = _multiConduitInfos[singleConduitInfo.MultiConduitId];

            // Segment references
            ResolveSegmentReferences(singleConduitInfo);

            #endregion

            // Update
            if (_singleConduitInfos.ContainsKey(singleConduitInfo.Id))
            {
                var existingSingleConduitInfo = _singleConduitInfos[singleConduitInfo.Id];

                // Update node to segment dictionary
                _pointOfInterestIndex.Update(existingSingleConduitInfo, singleConduitInfo);

                _mapper.Map<SingleConduitInfo, SingleConduitInfo>(singleConduitInfo, existingSingleConduitInfo);
            }
            // Insert
            else
            {
                // Update node to segment dictionary
                _pointOfInterestIndex.Update(null, singleConduitInfo);

                _singleConduitInfos.Add(singleConduitInfo.Id, singleConduitInfo);

                // If part of multi conduit add reference to it from that one
                if (singleConduitInfo.MultiConduitId != Guid.Empty)
                {
                    // Add to multi conduit children
                    if (_multiConduitInfos[singleConduitInfo.MultiConduitId].Children == null)
                        _multiConduitInfos[singleConduitInfo.MultiConduitId].Children = new List<ConduitInfo>();

                    _multiConduitInfos[singleConduitInfo.MultiConduitId].Children.Add(singleConduitInfo);
                }
            }
        }
        

        private void ResolveSegmentReferences(ConduitInfo condutiInfo)
        {
            var conduitWalkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(condutiInfo.GetRootConduit().WalkOfInterestId);

            // Resolve references inside segment
            foreach (var segment in condutiInfo.Segments)
            {
                // Resolve conduit reference
                segment.Conduit = condutiInfo;

                // Resolve conduit segment parent/child relationship
                if (segment.Conduit.Kind == ConduitKindEnum.InnerConduit)
                {
                    // Create parents list if null
                    if (segment.Parents == null)
                        segment.Parents = new List<ConduitSegmentInfo>();

                    var innerConduitSegmentWalkOfInterest = conduitWalkOfInterest.SubWalk2(segment.FromNodeId, segment.ToNodeId);

                    var multiConduit = segment.Conduit.Parent;

                    // Go through each segment of the multi conduit to find if someone intersects with the inner conduit segment
                    foreach (var multiConduitSegment in multiConduit.Segments)
                    {
                        // Create childre list if null
                        if (multiConduitSegment.Children == null)
                            multiConduitSegment.Children = new List<ConduitSegmentInfo>();

                        var multiConduitSegmentWalkOfInterest = conduitWalkOfInterest.SubWalk2(multiConduitSegment.FromNodeId, multiConduitSegment.ToNodeId);

                        // Create hash set for quick lookup
                        HashSet<Guid> multiConduitSegmentWalkOfInterestSegmetns = new HashSet<Guid>();
                        foreach (var segmentId in multiConduitSegmentWalkOfInterest.AllSegmentIds)
                            multiConduitSegmentWalkOfInterestSegmetns.Add(segmentId);
                        
                        // check if overlap from segments of the inner conduit to the the multi conduit segment
                        foreach (var innerConduitSegmentId in innerConduitSegmentWalkOfInterest.AllSegmentIds)
                        {
                            if (multiConduitSegmentWalkOfInterestSegmetns.Contains(innerConduitSegmentId))
                            {
                                if (!multiConduitSegment.Children.Contains(segment))
                                    multiConduitSegment.Children.Add(segment);

                                if (!segment.Parents.Contains(multiConduitSegment))
                                    segment.Parents.Add(multiConduitSegment);
                            }
                        }
                    }
                }

                // From Junction
                if (segment.FromJunctionId != Guid.Empty)
                {
                    if (!_singleConduitJuncionInfos.ContainsKey(segment.FromJunctionId))
                    {
                        var newJunction = new SingleConduitSegmentJunctionInfo() { Id = segment.FromJunctionId, ToConduitSegment = segment };
                        _singleConduitJuncionInfos.Add(newJunction.Id, newJunction);
                        segment.FromJunction = newJunction;
                    }
                    else
                    {
                        var existingJunction = _singleConduitJuncionInfos[segment.FromJunctionId];
                        existingJunction.ToConduitSegment = segment;
                        segment.FromJunction = existingJunction;
                    }
                }

                // To Junction
                if (segment.ToJunctionId != Guid.Empty)
                {
                    if (!_singleConduitJuncionInfos.ContainsKey(segment.ToJunctionId))
                    {
                        var newJunction = new SingleConduitSegmentJunctionInfo() { Id = segment.ToJunctionId, FromConduitSegment = segment };
                        _singleConduitJuncionInfos.Add(newJunction.Id, newJunction);
                        segment.ToJunction = newJunction;
                    }
                    else
                    {
                        var existingJunction = _singleConduitJuncionInfos[segment.ToJunctionId];
                        existingJunction.FromConduitSegment = segment;
                        segment.ToJunction = existingJunction;
                    }
                }
            }

        }

        #endregion

        public void Clean()
        {
            Load();
        }
    }
}
