using ConduitNetwork.ReadModel;
using DiagramLayout.Model;
using GraphQL.DataLoader;
using GraphQL.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QueryModel.Conduit;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class DiagramObjectType : ObjectGraphType<DiagramObject>
    {
        public DiagramObjectType(IDataLoaderContextAccessor dataLoader)
        {
            Description = "Diagram object";

            Field(x => x.Style, type: typeof(StringGraphType)).Description("Style name");
            Field(x => x.Label, type: typeof(StringGraphType)).Description("Label");

            Field<GeometryType>(
              "geometry",
              resolve: context =>
              {
                  return MapGeometry(context.Source.Geometry);
              });

            Field<IdGraphType>(
              "refId",
              resolve: context =>
              {
                  if (context.Source.IdentifiedObject != null)
                      return context.Source.IdentifiedObject.RefId;
                  return null;
              });

            Field<StringGraphType>(
            "refClass",
            resolve: context =>
            {
                if (context.Source.IdentifiedObject != null)
                    return context.Source.IdentifiedObject.RefClass;
                return null;
            });

        }

        private RouteNetwork.Events.Model.Geometry MapGeometry(NetTopologySuite.Geometries.Geometry geometry)
        {
            var writer = new NetTopologySuite.IO.GeoJsonWriter();
            var geometryJson = writer.Write(geometry);

            var geometryObj = JObject.Parse(geometryJson);

            var coordinatesJson = geometryObj["coordinates"].ToString(Formatting.None);
            var typeJson = geometryObj["type"].ToString(Formatting.None).Replace("\"", "");

            return new RouteNetwork.Events.Model.Geometry(typeJson, coordinatesJson);
        }
    }
}
