using Asset.Model;
using Core.ReadModel.Network;
using GraphQL.DataLoader;
using GraphQL.Types;
using QueryModel.Conduit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class NodeInterface : InterfaceGraphType<INode>
    {
        public NodeInterface(IDataLoaderContextAccessor dataLoader)
        {
            Description = "Interface for accessing general node information.";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");

            Field(x => x.Name, type: typeof(IdGraphType)).Description("Name of node managed by the utility");
        }
    }
}
