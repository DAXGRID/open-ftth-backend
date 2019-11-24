using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using ConduitNetwork.Events.Model;
using ConduitNetwork.ReadModel.ConduitClosure;
using Marten;

namespace ConduitNetwork.QueryService.ConduitClosure
{
    public class ConduitClosureRepository : IConduitClosureRepository
    {
        private IMapper _mapper = null;

        IDocumentStore documentStore = null;

        private IConduitNetworkQueryService conduitNetworkQueryService;

        private Dictionary<Guid, ConduitClosureInfo> _conduitClosureInfos = new Dictionary<Guid, ConduitClosureInfo>();

        private Dictionary<Guid, ConduitClosureInfo> _conduitClosureByPointOfInterestId = new Dictionary<Guid, ConduitClosureInfo>();

        private Dictionary<Guid, ConduitClosureInfo> _conduitClosureByLineId = new Dictionary<Guid, ConduitClosureInfo>();


        public ConduitClosureRepository(IDocumentStore documentStore, IConduitNetworkQueryService conduitNetworkQueryService)
        {
            this.documentStore = documentStore;
            this.conduitNetworkQueryService = conduitNetworkQueryService;

            // Initialize mapper
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<ConduitClosureInfo, ConduitClosureInfo>();
            });

            _mapper = config.CreateMapper();

            Load();
        }

        private void Load()
        {
            _conduitClosureInfos = new Dictionary<Guid, ConduitClosureInfo>();
            _conduitClosureByPointOfInterestId = new Dictionary<Guid, ConduitClosureInfo>();
            _conduitClosureByLineId = new Dictionary<Guid, ConduitClosureInfo>();

            // Read everything into memory for fast access
            using (var session = documentStore.LightweightSession())
            {
                var conduitClosures = session.Query<ConduitClosureInfo>();

                foreach (var conduitClosure in conduitClosures)
                {
                    UpdateConduitClosureInfo(conduitClosure);
                }
            }
        }

        public void Clean()
        {
            Load();
        }

        public void UpdateConduitClosureInfo(ConduitClosureInfo conduitClosureInfo)
        {
            // Update
            if (_conduitClosureInfos.ContainsKey(conduitClosureInfo.Id))
            {
                var existingConduitClosureInfo = _conduitClosureInfos[conduitClosureInfo.Id];

                _mapper.Map<ConduitClosureInfo, ConduitClosureInfo>(conduitClosureInfo, existingConduitClosureInfo);
            }
            // Insert
            else
            {
                _conduitClosureInfos.Add(conduitClosureInfo.Id, conduitClosureInfo);

                _conduitClosureByPointOfInterestId.Add(conduitClosureInfo.PointOfInterestId, conduitClosureInfo);
            }

            // Resolve references
            ResolveConduitSegmentReferences(conduitClosureInfo);

            // Index conduits
            IndexConduits(conduitClosureInfo);
        }

        private void IndexConduits(ConduitClosureInfo conduitClosureInfo)
        {
            foreach (var side in conduitClosureInfo.Sides)
            {
                foreach (var port in side.Ports)
                {
                    if (port.MultiConduitSegment != null && !_conduitClosureByLineId.ContainsKey(port.MultiConduitSegment.ConduitId))
                        _conduitClosureByLineId[port.MultiConduitSegment.ConduitId] = conduitClosureInfo;
                }
            }
        }

        private void ResolveConduitSegmentReferences(ConduitClosureInfo conduitClosureInfo)
        {
            foreach (var side in conduitClosureInfo.Sides)
            {
                foreach (var port in side.Ports)
                {
                    if (port.MultiConduitSegment == null && port.MultiConduitSegmentId != Guid.Empty)
                        port.MultiConduitSegment = (ConduitSegmentInfo)conduitNetworkQueryService.GetMultiConduitInfo(port.MultiConduitId).Segments.Find(s => s.Id == port.MultiConduitSegmentId);

                    foreach (var terminal in port.Terminals)
                    {
                        if (terminal.LineSegment == null && terminal.LineSegmentId != Guid.Empty)
                            terminal.LineSegment = conduitNetworkQueryService.GetSingleConduitInfo(terminal.LineId).Segments.Find(s => s.Id == terminal.LineSegmentId);

                    }
                }
            }
        }

        public void RemoveConduitClosureInfo(Guid id)
        {
            if (_conduitClosureInfos.ContainsKey(id))
            {
                var conduitClosure = _conduitClosureInfos[id];

                // Remove from point of interest dictionary
                if (_conduitClosureByPointOfInterestId.ContainsKey(conduitClosure.PointOfInterestId))
                    _conduitClosureByPointOfInterestId.Remove(conduitClosure.PointOfInterestId);

                // Remove from conduit closure dictionary
                _conduitClosureInfos.Remove(id);
            }

        }
        
        public ConduitClosureInfo GetConduitClosureInfo(Guid id)
        {
            if (_conduitClosureInfos.ContainsKey(id))
                return _conduitClosureInfos[id];

            throw new KeyNotFoundException("Cannot find any conduit closure info with id: " + id);
        }
        public ConduitClosureInfo GetConduitClosureInfoByRouteNodeId(Guid pointOfInterestId)
        {
            if (_conduitClosureByPointOfInterestId.ContainsKey(pointOfInterestId))
                return _conduitClosureByPointOfInterestId[pointOfInterestId];

            throw new KeyNotFoundException("Cannot find any conduit closure info in point of interest with id: " + pointOfInterestId);
        }

        public bool CheckIfRouteNodeContainsConduitClosure(Guid pointOfInterestId)
        {
            if (_conduitClosureByPointOfInterestId.ContainsKey(pointOfInterestId))
                return true;
            else
                return false;
        }

        public ConduitClosureInfo GetConduitClosureInfoByRelatedLineId(Guid lineId)
        {
            if (_conduitClosureByLineId.ContainsKey(lineId))
                return _conduitClosureByLineId[lineId];

            throw new KeyNotFoundException("Cannot find any conduit closure info related to line with id: " + lineId);
        }

        public bool CheckIfConduitClosureIsRelatedToLine(Guid lineId)
        {
            if (_conduitClosureByLineId.ContainsKey(lineId))
                return true;
            else
                return false;
        }

        public bool CheckIfConduitClosureAlreadyExists(Guid conduitClosureId)
        {
            if (_conduitClosureInfos.ContainsKey(conduitClosureId))
                return true;
            else
                return false;
        }
    }
}
