using ConduitNetwork.Events.Model;
using GraphQL.DataLoader;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitShapeKindEnumType : EnumerationGraphType<ConduitShapeKindEnum>
    {
        public ConduitShapeKindEnumType()
        {
            Name = "ConduitShapeKind";
            Description = @"The conduit shape - i.e. round, flat etc.";
        }
    }
}
