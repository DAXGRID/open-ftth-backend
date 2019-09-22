using GraphQL.Types;
using QueryModel.Conduit;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class RouteNodeKindEnumType : EnumerationGraphType<RouteNodeKindEnum>
    {
        public RouteNodeKindEnumType()
        {
            Name = "RouteNodeKind";
            Description = @"The kind  of route node - i.e. central office, cabinet etc.";
        }
    }
}
