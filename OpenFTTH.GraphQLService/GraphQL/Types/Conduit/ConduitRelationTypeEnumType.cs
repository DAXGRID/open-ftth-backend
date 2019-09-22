using ConduitNetwork.ReadModel;
using GraphQL.Types;
using QueryModel.Conduit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitRelationTypeEnumType : EnumerationGraphType<ConduitRelationTypeEnum>
    {
        public ConduitRelationTypeEnumType()
        {
            Name = "ConduitRelationEnum";
            Description = @"The type of relationship between the outer conduit and the route network object. " +
                "For an outer conduit related to a segment, this value will always be PASS_THROUGH, because it's not possible to do anything with conduits placed in a route segment data modelling wise, " +
                "without splitting the segment and adding a node first. " +
                "For nodes, an outer conduit can either PASS_BY (i.e. just passing around the cabinet, well or conduit junction), PASS_THROUGH (i.e. pass through a cabinet, well or conduit junction), INCOMMING (the end of the conduit enters the the node), or OUTGOING (the start of the conduit goes out from the node)";
        }
    }
}
