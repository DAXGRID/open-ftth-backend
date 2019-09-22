using GraphQL.DataLoader;
using GraphQL.Types;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class GeometryType : ObjectGraphType<Geometry>
    {
        public GeometryType(IDataLoaderContextAccessor dataLoader)
        {
            Description = "A GeoJson Geometry";

            Field(x => x.GeoJsonType, type: typeof(IdGraphType)).Name("type").Description("Type of the geometry");
            Field(x => x.GeoJsonCoordinates, type: typeof(IdGraphType)).Name("coordinates").Description("Coordinates of the geometry");
        }
    }
}
