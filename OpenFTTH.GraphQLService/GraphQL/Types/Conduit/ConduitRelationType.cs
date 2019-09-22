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
        }
    }
}
