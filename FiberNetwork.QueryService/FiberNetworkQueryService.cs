using AutoMapper;
using Core.ReadModel.Network;
using FiberNetwork.Events.Model;
using Marten;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiberNetwork.QueryService
{
    public class FiberNetworkQueryService : IFiberNetworkQueryService
    {
        private IDocumentStore documentStore = null;

        private PointOfInterestIndex _pointOfInterestIndex;

        private IRouteNetworkState routeNetworkQueryService = null;

        private Dictionary<Guid, FiberCableInfo> _fiberCableInfos = new Dictionary<Guid, FiberCableInfo>();
        
        private IMapper _mapper = null;


        public FiberNetworkQueryService(IDocumentStore documentStore, IRouteNetworkState routeNetworkQueryService)
        {
            this.documentStore = documentStore;
            this.routeNetworkQueryService = routeNetworkQueryService;
            this._pointOfInterestIndex = new PointOfInterestIndex(routeNetworkQueryService);
          

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<FiberCableInfo, FiberCableInfo>();
            });


            _mapper = config.CreateMapper();

            Load();

        }

        private void Load()
        {
            _fiberCableInfos = new Dictionary<Guid, FiberCableInfo>();
            _pointOfInterestIndex = new PointOfInterestIndex(routeNetworkQueryService);

            using (var session = documentStore.LightweightSession())
            {
                // Fetch everything into memory for fast access
                var fiberCables = session.Query<FiberCableInfo>();

                foreach (var fiberCable in fiberCables)
                {
                    UpdateMultiConduitInfo(fiberCable);
                }
            }
        }

        public bool CheckIfFiberCableIdExists(Guid id)
        {
            return _fiberCableInfos.ContainsKey(id);
        }

        public bool CheckIfConduitIsCut(Guid conduitId, Guid pointOfInterestId)
        {
            FiberInfo fiberCableToCheck = null;

            if (_fiberCableInfos.ContainsKey(conduitId))
                fiberCableToCheck = _fiberCableInfos[conduitId];
            else
            {
                throw new KeyNotFoundException("Cannot find any fiber cable with id: " + conduitId);
            }

            // Check if fiber cable is cut
            if (fiberCableToCheck.Segments.Exists(s => s.FromRouteNodeId == pointOfInterestId || s.ToRouteNodeId == pointOfInterestId))
                return true;
            else
                return false;
        }

        public FiberCableInfo GetFiberCableInfo(Guid id)
        {
            if (_fiberCableInfos.ContainsKey(id))
                return _fiberCableInfos[id];

            throw new KeyNotFoundException("Cannot find any fiber cable with id: " + id);
        }

        /*
        public SingleConduitSegmentJunctionInfo GetSingleConduitSegmentJunctionInfo(Guid id)
        {
            if (_singleConduitJuncionInfos.ContainsKey(id))
                return _singleConduitJuncionInfos[id];

            throw new KeyNotFoundException("Cannot find any single conduit segment conduit info with id: " + id);
        }
        */

        public List<FiberRelationInfo> GetConduitSegmentsRelatedToPointOfInterest(Guid pointOfInterestId, string conduitId = null)
        {
            List<FiberRelationInfo> result = new List<FiberRelationInfo>();

            var fiberSegments = _pointOfInterestIndex.GetConduitSegmentsThatEndsInRouteNode(pointOfInterestId);

            foreach (var fiberSegment in fiberSegments)
            {
                // If conduit segment id set, skip until we read conduit segment specified
                if (conduitId != null)
                {
                    var idToCheck = Guid.Parse(conduitId);

                    if (fiberSegment.Id != idToCheck && fiberSegment.Conduit.Id != idToCheck)
                        continue;
                }

                FiberRelationInfo relInfo = new FiberRelationInfo();

                if (fiberSegment.ToRouteNodeId == pointOfInterestId)
                    result.Add(new FiberRelationInfo() { Segment = fiberSegment, Type = FiberRelationTypeEnum.Incomming });

                if (fiberSegment.FromRouteNodeId == pointOfInterestId)
                    result.Add(new FiberRelationInfo() { Segment = fiberSegment, Type = FiberRelationTypeEnum.Outgoing });

            }

            var fiberSegmentPassBy = _pointOfInterestIndex.GetConduitSegmentsThatPassedByRouteNode(pointOfInterestId);

            foreach (var fiberSegment in fiberSegmentPassBy)
            {
                // If conduit segment id set, skip until we read conduit segment specified
                if (conduitId != null)
                {
                    var idToCheck = Guid.Parse(conduitId);

                    if (fiberSegment.Id != idToCheck && fiberSegment.Conduit.Id != idToCheck)
                        continue;
                }

                result.Add(new FiberRelationInfo() { Segment = fiberSegment, Type = FiberRelationTypeEnum.PassThrough });
            }

            return result;
        }


        public List<FiberRelationInfo> GetConduitSegmentsRelatedToRouteSegment(Guid routeSegmentId, string conduitId = null)
        {
            List<FiberRelationInfo> result = new List<FiberRelationInfo>();

            var fiberSegments = _pointOfInterestIndex.GetConduitSegmentsThatPassedByRouteSegment(routeSegmentId);

            foreach (var conduitSegment in fiberSegments)
            {
                // If conduit segment id set, skip until we read conduit segment specified
                if (conduitId != null)
                {
                    var idToCheck = Guid.Parse(conduitId);

                    if (conduitSegment.Id != idToCheck)
                        continue;
                }

                result.Add(new FiberRelationInfo() { Segment = conduitSegment, Type = FiberRelationTypeEnum.PassThrough });
            }

            return result;
        }



        #region utility functions that can be used to create derived info objects
        /*
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

            result.AllRouteNodes = new List<RouteNodeInfo>();
            foreach (var nodeId in allNodeIds)
                result.AllRouteNodes.Add(routeNetworkQueryService.GetRouteNodeInfo(nodeId));

            result.AllRouteSegments = new List<RouteSegmentInfo>();
            foreach (var segmentId in allSegmentIds)
                result.AllRouteSegments.Add(routeNetworkQueryService.GetRouteSegmentInfo(segmentId));

            result.AllConduitSegments = conduitSegments.ToList();

            return result;
        }
        */

        #endregion


        #region functions called during projection and snapshot reading

        public void UpdateMultiConduitInfo(FiberCableInfo fiberCableInfo, bool load = false)
        {
            // Resolve segment references
            ResolveSegmentReferences(fiberCableInfo);

            // Update
            if (_fiberCableInfos.ContainsKey(fiberCableInfo.Id))
            {
                var existingFiberCableInfo = _fiberCableInfos[fiberCableInfo.Id];

                // Update node to segment dictionary
                _pointOfInterestIndex.Update(existingFiberCableInfo, fiberCableInfo);

                // Save the children
                fiberCableInfo.Children = new List<ILine>();
                fiberCableInfo.Children.AddRange(existingFiberCableInfo.Children);

                _mapper.Map<FiberCableInfo, FiberCableInfo>(fiberCableInfo, existingFiberCableInfo);


            }
            // Insert
            else
            {
                // Update node to segment dictionary
                _pointOfInterestIndex.Update(null, fiberCableInfo);

                _fiberCableInfos.Add(fiberCableInfo.Id, fiberCableInfo);
            }
        }

        private void ResolveSegmentReferences(FiberInfo fiberInfo)
        {
            /*
            var conduitWalkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(fiberInfo.GetRoot().WalkOfInterestId);

            // Resolve references inside segment
            foreach (var segment in fiberInfo.Segments.OfType<FiberSegmentInfo>())
            {
                // Resolve conduit reference
                segment.Conduit = fiberInfo;

                // Resolve conduit segment parent/child relationship
                if (segment.Conduit.Kind == ConduitKindEnum.InnerConduit)
                {
                    // Create parents list if null
                    if (segment.Parents == null)
                        segment.Parents = new List<ILineSegment>();

                    var innerConduitSegmentWalkOfInterest = conduitWalkOfInterest.SubWalk2(segment.FromRouteNodeId, segment.ToRouteNodeId);

                    var multiConduit = segment.Conduit.Parent;

                    // Go through each segment of the multi conduit to find if someone intersects with the inner conduit segment
                    foreach (var multiConduitSegment in multiConduit.Segments)
                    {
                        // Create childre list if null
                        if (multiConduitSegment.Children == null)
                            multiConduitSegment.Children = new List<ILineSegment>();

                        var multiConduitSegmentWalkOfInterest = conduitWalkOfInterest.SubWalk2(multiConduitSegment.FromRouteNodeId, multiConduitSegment.ToRouteNodeId);

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
                if (segment.FromNodeId != Guid.Empty)
                {
                    if (!_singleConduitJuncionInfos.ContainsKey(segment.FromNodeId))
                    {
                        var newJunction = new SingleConduitSegmentJunctionInfo() { Id = segment.FromNodeId };
                        newJunction.AddToConduitSegment(segment);
                        _singleConduitJuncionInfos.Add(newJunction.Id, newJunction);
                        segment.FromNode = newJunction;
                    }
                    else
                    {
                        var existingJunction = _singleConduitJuncionInfos[segment.FromNodeId];
                        //existingJunction.ToConduitSegments = segment;
                        existingJunction.AddToConduitSegment(segment);
                        segment.FromNode = existingJunction;
                    }
                }

                // To Junction
                if (segment.ToNodeId != Guid.Empty)
                {
                    if (!_singleConduitJuncionInfos.ContainsKey(segment.ToNodeId))
                    {
                        var newJunction = new SingleConduitSegmentJunctionInfo() { Id = segment.ToNodeId };
                        newJunction.AddFromConduitSegment(segment);
                        _singleConduitJuncionInfos.Add(newJunction.Id, newJunction);
                        segment.ToNode = newJunction;
                    }
                    else
                    {
                        var existingJunction = _singleConduitJuncionInfos[segment.ToNodeId];
                        existingJunction.AddFromConduitSegment(segment);
                        segment.ToNode = existingJunction;
                    }
                }
            }
            */

        }

        #endregion

        public void Clean()
        {
            Load();
        }
    }
}
