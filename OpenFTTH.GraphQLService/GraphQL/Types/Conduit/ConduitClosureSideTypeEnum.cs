using ConduitNetwork.Events.Model;
using GraphQL.Types;
using QueryModel.Conduit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitClosureSideEnumType : EnumerationGraphType<ConduitClosureInfoSide>
    {
        public ConduitClosureSideEnumType()
        {
            Name = "ConduitClosureSideEnum";
            Description = @"The side (left, right, top or bottom) of conduit closure.";
        }
    }
}
