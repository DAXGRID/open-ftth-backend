using ConduitNetwork.Events.Model;
using ConduitNetwork.ReadModel.ConduitClosure;
using GraphQL.Types;
using QueryModel.Conduit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitClosureInternalConnectionKindType : EnumerationGraphType<ConduitClosureInternalConnectionKindEnum>
    {
        public ConduitClosureInternalConnectionKindType()
        {
            Name = "ConduitClosureInternalConnectionkind";
            Description = @"The type of connection this port or teminal has to another port or terminal in the closure. NotConnected means that there is no connection from the line segment in this port/terminal to another port/terminal. Connected means that the line segment in this port/terminal is connected to another line segment in another port/terminal. PassThrough means that the line segment of this port/terminal is passing through the another port/terminal.";
        }
    }
}
