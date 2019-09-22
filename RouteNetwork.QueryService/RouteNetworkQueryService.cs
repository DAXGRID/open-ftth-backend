using GeoCoordinatePortable;
using Marten;
using NetTopologySuite.Geometries;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.QueryService
{
    public class RouteNetworkQueryService : IRouteNetworkQueryService
    {
        private IDocumentStore documentStore;
        private Dictionary<Guid, RouteNodeInfo> _routeNodeInfos = new Dictionary<Guid, RouteNodeInfo>();
        private Dictionary<Guid, RouteSegmentInfo> _routeSegmentInfos = new Dictionary<Guid, RouteSegmentInfo>();
        private Dictionary<Guid, WalkOfInterestInfo> _walkOfInterests = new Dictionary<Guid, WalkOfInterestInfo>();
        

        public RouteNetworkQueryService(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;

            Load();
        }

        private void Load()
        {
            _routeNodeInfos = new Dictionary<Guid, RouteNodeInfo>();
            _routeSegmentInfos = new Dictionary<Guid, RouteSegmentInfo>();
            _walkOfInterests = new Dictionary<Guid, WalkOfInterestInfo>();

            using (var session = documentStore.LightweightSession())
            {
                // Fetch everything into memory for fast access
                var routeNodeInfoQuery = session.Query<RouteNodeInfo>();

                foreach (var routeNodeInfo in routeNodeInfoQuery)
                {
                    AddRouteNodeInfo(routeNodeInfo);
                }

                var routeSegmentInfoQuery = session.Query<RouteSegmentInfo>();

                foreach (var routeSegmentInfo in routeSegmentInfoQuery)
                {
                    AddRouteSegmentInfo(routeSegmentInfo);
                }

                var walkOfInterests = session.Query<WalkOfInterestInfo>();

                foreach (var walkOfInterest in walkOfInterests)
                {
                    AddWalkOfInterestInfo(walkOfInterest);
                }
            }
        }

        public bool CheckIfRouteNodeIdExists(Guid id)
        {
            return _routeNodeInfos.ContainsKey(id);
        }

        public bool CheckIfRouteSegmentIdExists(Guid id)
        {
            return _routeSegmentInfos.ContainsKey(id);
        }

        public IRouteElementInfo GetRouteElementInfo(Guid id)
        {
            if (_routeSegmentInfos.ContainsKey(id))
                return _routeSegmentInfos[id];

            if (_routeNodeInfos.ContainsKey(id))
                return _routeNodeInfos[id];

           throw new KeyNotFoundException("Cannot find any route element info with id: " + id);
        }

        public RouteNodeInfo GetRouteNodeInfo(Guid id)
        {
            if (!_routeNodeInfos.ContainsKey(id))
                throw new KeyNotFoundException("Cannot find any route node info with id: " + id);

            return _routeNodeInfos[id];
        }

        public RouteSegmentInfo GetRouteSegmentInfo(Guid id)
        {
            if (!_routeSegmentInfos.ContainsKey(id))
                throw new KeyNotFoundException("Cannot find any route segment info with id: " + id);

            return _routeSegmentInfos[id];
        }

        public WalkOfInterestInfo GetWalkOfInterestInfo(Guid id)
        {
            if (!_walkOfInterests.ContainsKey(id))
                throw new KeyNotFoundException("Cannot find any walk of interest with id: " + id);

            return _walkOfInterests[id];
        }

        public IEnumerable<RouteNodeInfo> GetAllRouteNodes()
        {
            return _routeNodeInfos.Values;
        }
        public IEnumerable<RouteSegmentInfo> GetAllRouteSegments()
        {
            return _routeSegmentInfos.Values;
        }

        public void AddRouteNodeInfo(RouteNodeInfo routeNodeInfo)
        {
            _routeNodeInfos.Add(routeNodeInfo.Id, routeNodeInfo);
        }

        public void AddRouteSegmentInfo(RouteSegmentInfo routeSegmentInfo)
        {
            _routeSegmentInfos.Add(routeSegmentInfo.Id, routeSegmentInfo);

            var reader = new NetTopologySuite.IO.GeoJsonReader();

            // Calculate length
            var line = reader.Read<LineString>("{ \"type\": \"LineString\", \"coordinates\": " + routeSegmentInfo.Geometry.GeoJsonCoordinates + "}");

            double length = 0;

            for (int i = 1; i < line.NumPoints; i++)
            {
                var sCoord = new GeoCoordinate(line.GetPointN(i-1).Y, line.GetPointN(i - 1).X);
                var eCoord = new GeoCoordinate(line.GetPointN(i).Y, line.GetPointN(i).X);
                length += sCoord.GetDistanceTo(eCoord); 
            }

            routeSegmentInfo.Length = length;

            // Add object relations to facilitate easier and faster lookup and traversal
            routeSegmentInfo.FromNode = GetRouteNodeInfo(routeSegmentInfo.FromNodeId);
            routeSegmentInfo.ToNode = GetRouteNodeInfo(routeSegmentInfo.ToNodeId);

            routeSegmentInfo.FromNode.AddOutgoingSegment(routeSegmentInfo);
            routeSegmentInfo.ToNode.AddIngoingSegment(routeSegmentInfo);
        }

        public void AddWalkOfInterestInfo(WalkOfInterestInfo walkOfInterestInfo)
        {
            _walkOfInterests.Add(walkOfInterestInfo.Id, walkOfInterestInfo);

            // Add object relations to facilitate easier and faster lookup and traversal
            walkOfInterestInfo.RouteElements = new List<Core.GraphSupport.Model.GraphElement>();

            bool isNode = true;
            foreach (var routeElementId in walkOfInterestInfo.RouteElementIds)
            {
                if (isNode)
                {
                    var node = _routeNodeInfos[routeElementId];
                    node.AddWalkOfInterest(walkOfInterestInfo);
                    walkOfInterestInfo.RouteElements.Add(node);
                }
                else
                {
                    var segment = _routeSegmentInfos[routeElementId];
                    segment.AddWalkOfInterest(walkOfInterestInfo);
                    walkOfInterestInfo.RouteElements.Add(segment);
                }

                if (isNode)
                    isNode = false;
                else
                    isNode = true;
            }
        }

        public void Clean()
        {
            Load();
        }
    }
}
