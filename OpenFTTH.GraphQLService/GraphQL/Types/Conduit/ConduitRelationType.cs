using GraphQL.DataLoader;
using GraphQL.Types;
using QueryModel.Conduit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitRelationType : ObjectGraphType<ConduitRelation>
    {
        public ConduitRelationType(IDataLoaderContextAccessor dataLoader)
        {
            Description = "Used to query conduits related to a route network object (a node or segment). ";
            
            Field<ConduitRelationTypeEnumType>("RelationType", "Type of conduit relation.");

            Field<ConduitSegmentType>("ConduitSegment", "The segment of the conduit that is related to this route object.");

            Field<ConduitInfoType>("Conduit", "The conduit object that that is related to this route object.");

            Field(x => x.CanBeAttachedToConduitClosure, type: typeof(BooleanGraphType)).Description("True if conduit is attached to a conduit closure inside the node queried.");

            Field(x => x.CanBeCutAtNode, type: typeof(BooleanGraphType)).Description("True if conduit is cut at the node queried.");
        }
    }
}
