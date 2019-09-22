using Infrastructure.EventSourcing;
using Location.Model;
using NetTopologySuite.Geometries;
using RouteNetwork.Events;
using RouteNetwork.Events.Model;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Business.Aggregates
{
    public class RouteNode : AggregateBase
    {
        private RouteNode()
        {
            // Register the event types that make up our aggregate , together with their respective handlers
            Register<RouteNodeAdded>(Apply);
        }

        internal RouteNode(Guid routeNodeId, string name, RouteNodeKindEnum nodeKind, RouteNodeFunctionKindEnum functionKind, RouteNetwork.Events.Model.Geometry geometry, LocationInfo locationInfo) : this()
        {
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
            var reader = new NetTopologySuite.IO.GeoJsonReader();

            try
            {
                var point = reader.Read<Point>("{ \"type\": \"Point\", \"coordinates\": " + geometry.GeoJsonCoordinates + "}");

                if (point == null)
                    throw new ArgumentException("Error parsing geometry: " + geometry);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error parsing geometry: " + geometry + " Got exception from NetTopologySuite: " + ex.Message, ex);
            }


            // Create domain event
            var routeNodeAdded = new RouteNodeAdded()
            {
                RouteNodeId = routeNodeId,
                Name = name,
                NodeKind = nodeKind,
                NodeFunctionKind = functionKind,
                InitialGeometry = geometry,
                LocationInfo = locationInfo
            };

            RaiseEvent(routeNodeAdded);
        }

        // Apply route node added event
        private void Apply(RouteNodeAdded @event)
        {
            Id = @event.RouteNodeId;
        }
    }
}
