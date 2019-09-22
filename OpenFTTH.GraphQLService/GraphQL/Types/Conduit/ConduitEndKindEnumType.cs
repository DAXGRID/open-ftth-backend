using ConduitNetwork.Events.Model;
using GraphQL.Types;
using QueryModel.Conduit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitEndKindEnumType : EnumerationGraphType<ConduitEndKindEnum>
    {
        public ConduitEndKindEnumType()
        {
            Name = "ConduitEndkindEnum";
            Description = @"Either the incomming or outgoing end.";
        }
    }
}
