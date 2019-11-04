using GeoAPI.Geometries;
using Infrastructure.EventSourcing;
using NetTopologySuite.Geometries;
using RouteNetwork.Events;
using RouteNetwork.Events.Model;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Business.Aggregates
{
    public class RouteSegment : AggregateBase
    {
        private RouteSegment()
        {
            // Register the event types that make up our aggregate , together with their respective handlers
            Register<RouteSegmentPlanned>(Apply);
            Register<PlannedRouteSegmentSplitByNode>(Apply);
        }

        internal RouteSegment(Guid routeSegmentId, Guid fromNodeId, Guid toNodeId, RouteSegmentKindEnum segmentKind, RouteNetwork.Events.Model.Geometry geometry) : this()
        {
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
            var line = ConvertFromLineGeoJson(geometry.GeoJsonCoordinates);

            // Create domain event
            var routeNodeAdded = new RouteSegmentPlanned()
            {
                Id = routeSegmentId,
                FromNodeId = fromNodeId,
                ToNodeId = toNodeId,
                SegmentKind = segmentKind,
                InitialGeometry = geometry
            };

            RaiseEvent(routeNodeAdded);
        }

        public void Split(Guid nodeId, IRouteNetworkState routeQueryService)
        {
            var segmentInfo = routeQueryService.GetRouteSegmentInfo(this.Id);

            var nodeInfo = routeQueryService.GetRouteNodeInfo(nodeId);

            // Check that split node is close enough to the segment to be splitted
            var line = ConvertFromLineGeoJson(segmentInfo.Geometry.GeoJsonCoordinates);

            var nodePoint = ConvertFromPointGeoJson(nodeInfo.Geometry.GeoJsonCoordinates);

            var test = line.Distance(nodePoint);

            if (test > 0.000001)
            {
                throw new ArgumentException("Coordinate of node used for splitting the segment is not within allowed distance to segment. The distance is greather than 0.000001 decimal degress (around 5-10 cm) which is not allowed.");
            }

            PlannedRouteSegmentSplitByNode segmentSplitEvent = new PlannedRouteSegmentSplitByNode() { Id = this.Id, SplitNodeId = nodeId };

            RaiseEvent(segmentSplitEvent);
        }


        // Apply route segment added event
        private void Apply(RouteSegmentPlanned @event)
        {
            Id = @event.Id;
        }

        private void Apply(PlannedRouteSegmentSplitByNode @event)
        {
        }

        private Point ConvertFromPointGeoJson(string geojson)
        {
            try
            {
                var reader = new NetTopologySuite.IO.GeoJsonReader();
                var point = reader.Read<Point>("{ \"type\": \"Point\", \"coordinates\": " + geojson + "}");
                return point;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error parsing geometry: " + geojson + " Got exception from NetTopologySuite: " + ex.Message, ex);
            }
        }

        private LineString ConvertFromLineGeoJson(string geojson)
        {
            try
            {
                var reader = new NetTopologySuite.IO.GeoJsonReader();
                var line = reader.Read<LineString>("{ \"type\": \"LineString\", \"coordinates\": " + geojson + "}");
                return line;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error parsing geometry: " + geojson + " Got exception from NetTopologySuite: " + ex.Message, ex);
            }
        }
    }
}
