using ConduitNetwork.Events.Model;
using GraphQL.Types;
using QueryModel.Conduit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitKindEnumType : EnumerationGraphType<ConduitKindEnum>
    {
        public ConduitKindEnumType()
        {
            Name = "Conduitkind";
            Description = @"The type of conduit. Can eithed be a multi conduit (that contains inner conduits), an inner conduit (which is just the name for a single conduit that is a child of a multi conduit), or a single conduit (which is a normal simple conduit not part of a multi conduit).";
        }
    }
}
