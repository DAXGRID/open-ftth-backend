using ConduitNetwork.ReadModel;
using DiagramLayout.Model;
using GraphQL.DataLoader;
using GraphQL.Types;
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
        }

        private RouteNetwork.Events.Model.Geometry MapGeometry(NetTopologySuite.Geometries.Geometry geometry)
        {
            var writer = new NetTopologySuite.IO.GeoJsonWriter();
            var geometryJson = writer.Write(geometry);


            return new RouteNetwork.Events.Model.Geometry("","");
        }
    }
}
