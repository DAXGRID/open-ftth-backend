using Infrastructure.EventSourcing;
using NetTopologySuite.Geometries;
using RouteNetwork.Events;
using RouteNetwork.Events.Model;
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
            Register<RouteSegmentAdded>(Apply);
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
            var reader = new NetTopologySuite.IO.GeoJsonReader();

            try
            {
                var line = reader.Read<LineString>("{ \"type\": \"LineString\", \"coordinates\": " + geometry.GeoJsonCoordinates + "}");

                if (line == null)
                    throw new ArgumentException("Error parsing geometry: " + geometry);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error parsing geometry: " + geometry + " Got exception from NetTopologySuite: " + ex.Message, ex);
            }


            // Create domain event
            var routeNodeAdded = new RouteSegmentAdded()
            {
                RouteSegmentId = routeSegmentId,
                FromNodeId = fromNodeId,
                ToNodeId = toNodeId,
                SegmentKind = segmentKind,
                InitialGeometry = geometry
            };

            RaiseEvent(routeNodeAdded);
        }

        // Apply route segment added event
        private void Apply(RouteSegmentAdded @event)
        {
            Id = @event.RouteSegmentId;
        }

    }
}
