using Infrastructure.EventSourcing;
using Location.Model;
using NetTopologySuite.Geometries;
using RouteNetwork.Business.GeometryUtil;
using RouteNetwork.Events;
using RouteNetwork.Events.Model;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Business.Aggregates
{
    public class RouteNetworkAggregate : AggregateBase
    {
        /// <summary>
        /// All state is maintained by the RouteNetworkState object
        /// </summary>
        IRouteNetworkState routeNetworkState;

        public RouteNetworkAggregate(IAggregateRepository aggregateRepository, IRouteNetworkState routeNetworkState)
        {
            this.routeNetworkState = routeNetworkState;
        }

        /// <summary>
        /// Add a new node to the route network
        /// </summary>
        /// <param name="routeNodeId"></param>
        /// <param name="name"></param>
        /// <param name="nodeKind"></param>
        /// <param name="functionKind"></param>
        /// <param name="geometry"></param>
        /// <param name="locationInfo"></param>
        public void AddRouteNode(Guid routeNodeId, string name, RouteNodeKindEnum nodeKind, RouteNodeFunctionKindEnum functionKind, RouteNetwork.Events.Model.Geometry geometry, LocationInfo locationInfo)
        {
            // Id check
            if (routeNodeId == null || routeNodeId == Guid.Empty)
                throw new ArgumentException("Id cannot be null or empty");

            // Check that node not already exists
            if (routeNetworkState.CheckIfRouteNodeIdExists(routeNodeId))
                throw new ArgumentException("A route node with id: " + routeNodeId + " already exists");

            // Check that we got some valid geometry
            if (geometry == null)
                throw new ArgumentException("Cannot create route node with id: " + routeNodeId + " because Geometry is null, which is not allowed.");

            if (geometry.GeoJsonType == null)
                throw new ArgumentException("Cannot create route node with id: " + routeNodeId + " because Geometry.GeoJsonType is null, which is not allowed.");

            if (geometry.GeoJsonType.ToLower() != "point")
                throw new ArgumentException("Cannot create route node with id: " + routeNodeId + " because Geometry.GeoJsonType is: " + geometry.GeoJsonType + ", which is not allowed in route nodes. Expected Point.");

            if (geometry.GeoJsonCoordinates == null)
                throw new ArgumentException("Cannot create route node with id: " + routeNodeId + " because Geometry.GeoJsonCoordinates is null, which is not allowed.");

            // Try parse geojson
            var point = GeometryConversionHelper.ConvertFromPointGeoJson(geometry.GeoJsonCoordinates);

            // Create domain event
            var routeNodePlanned = new RouteNodePlanned()
            {
                Id = routeNodeId,
                Name = name,
                NodeKind = nodeKind,
                NodeFunctionKind = functionKind,
                InitialGeometry = geometry,
                LocationInfo = locationInfo
            };

            RaiseEvent(routeNodePlanned, false);
        }

        /// <summary>
        /// Add a new route segment to the network
        /// </summary>
        /// <param name="routeSegmentId"></param>
        /// <param name="fromNodeId"></param>
        /// <param name="toNodeId"></param>
        /// <param name="segmentKind"></param>
        /// <param name="geometry"></param>
        public void AddRouteSegment(Guid routeSegmentId, Guid fromNodeId, Guid toNodeId, RouteSegmentKindEnum segmentKind, RouteNetwork.Events.Model.Geometry geometry)
        {
            // Id check
            if (routeSegmentId == null || routeSegmentId == Guid.Empty)
                throw new ArgumentException("Id cannot be null or empty");

            // Check that segment not already exists
            if (routeNetworkState.CheckIfRouteSegmentIdExists(routeSegmentId))
                throw new ArgumentException("A route segment with id: " + routeSegmentId + " already exists");

            // Check if from node exists
            if (!routeNetworkState.CheckIfRouteNodeIdExists(fromNodeId))
                throw new ArgumentException("The FromNodeId parameter is invalid. Cannot find any node in the network with id: " + fromNodeId);

            // Check if to node exists
            if (!routeNetworkState.CheckIfRouteNodeIdExists(toNodeId))
                throw new ArgumentException("The ToNodeId parameter is invalid. Cannot find any node in the network with id: " + toNodeId);

            // Check that we got some valid geometry
            if (geometry == null)
                throw new ArgumentException("Cannot create route segment with id: " + routeSegmentId + " because Geometry is null, which is not allowed.");

            if (geometry.GeoJsonType == null)
                throw new ArgumentException("Cannot create route segment with id: " + routeSegmentId + " because Geometry.GeoJsonType is null, which is not allowed.");

            if (geometry.GeoJsonType.ToLower() != "linestring")
                throw new ArgumentException("Cannot create route segment with id: " + routeSegmentId + " because Geometry.GeoJsonType is: " + geometry.GeoJsonType + ", which is not allowed in route segments. Expected LineString.");

            if (geometry.GeoJsonCoordinates == null)
                throw new ArgumentException("Cannot create route segment with id: " + routeSegmentId + " because Geometry.GeoJsonCoordinates is null, which is not allowed.");

            // Try parse geojson
            var line =  GeometryConversionHelper.ConvertFromLineGeoJson(geometry.GeoJsonCoordinates);

            // Create domain event
            var routeSegmentPlanned = new RouteSegmentPlanned()
            {
                Id = routeSegmentId,
                FromNodeId = fromNodeId,
                ToNodeId = toNodeId,
                SegmentKind = segmentKind,
                InitialGeometry = geometry
            };

            RaiseEvent(routeSegmentPlanned, false);
        }

        public void SplitRouteSegment(Guid routeSegmentId, Guid splittingNodeId)
        {
            // Id check
            if (routeSegmentId == null || routeSegmentId == Guid.Empty)
                throw new ArgumentException("Id cannot be null or empty");

            // Check that segment exists
            if (!routeNetworkState.CheckIfRouteSegmentIdExists(routeSegmentId))
                throw new ArgumentException("Cannot find any segment in the network with id:" + routeSegmentId);

            // Check that node exists
            if (!routeNetworkState.CheckIfRouteNodeIdExists(splittingNodeId))
                throw new ArgumentException("Cannot find any node in the network with id: " + splittingNodeId);

            var segmentInfo = routeNetworkState.GetRouteSegmentInfo(routeSegmentId);

            var nodeInfo = routeNetworkState.GetRouteNodeInfo(splittingNodeId);

            // Check that split node is close enough to the segment to be splitted
            var line = GeometryConversionHelper.ConvertFromLineGeoJson(segmentInfo.Geometry.GeoJsonCoordinates);

            var nodePoint = GeometryConversionHelper.ConvertFromPointGeoJson(nodeInfo.Geometry.GeoJsonCoordinates);

            var test = line.Distance(nodePoint);

            if (test > 0.000001)
            {
                throw new ArgumentException("Coordinate of node used for splitting the segment is not within allowed distance to segment. The distance is greather than 0.000001 decimal degress (around 5-10 cm) which is not allowed.");
            }

            PlannedRouteSegmentSplitByNode segmentSplitEvent = new PlannedRouteSegmentSplitByNode() { Id = routeSegmentId, SplitNodeId = splittingNodeId };

            RaiseEvent(segmentSplitEvent,false);
        }

    }
}
