using GraphQL.Types;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class RouteNodeFunctionKindEnumType : EnumerationGraphType<RouteNodeFunctionKindEnum>
    {
        public RouteNodeFunctionKindEnumType()
        {
            Name = "RouteNodeFunctionKind";
            Description = @"The kind of function the node do/supports. The primary and secondary nodes are central offices containing active equipment. A primary node is a CORE central office, with active equipment and core routers having uplink to the internet. " +
                "A secondary node represents a POP (point-of-presence) servicing customers in a given area. The secondary node (POP) is connected to the primary node (CORE) through a ring network known as the backbone. " +
                "A flex point is a node in the network (typical a street cabinet) with the possibility to both patch and splice. In a splice point there's no flexibilty in terms of patching. " +
                "A service delivery point is where customer premises equipment resides (inside a single or multi dwelling units). " +
                "An open conduit point is a man or hand hole that the field crew can access. " +
                "A burried conduit point is an underground conduit enclosure or branch-out - i.e. inaccessible unless you dig down to it.";

        }
    }
}
