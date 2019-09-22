using GraphQL.Types;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class RouteSegmentKindEnumType : EnumerationGraphType<RouteSegmentKindEnum>
    {
        public RouteSegmentKindEnumType()
        {
            Name = "RouteSegmentKind";
            Description = @"The kind of route segment - i.e. underground, drilling etc.";
        }
    }
}
