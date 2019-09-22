using ConduitNetwork.Events.Model;
using GraphQL.DataLoader;
using GraphQL.Types;
using QueryModel.Conduit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitColorEnumType : EnumerationGraphType<ConduitColorEnum>
    {
        public ConduitColorEnumType()
        {
            Name = "ConduitColorkind";
            Description = @"Colors used on conduits.";
        }
    }
}
