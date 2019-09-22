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
    public class NetworkElementInterface : InterfaceGraphType<INetworkElement>
    {
        public NetworkElementInterface(IDataLoaderContextAccessor dataLoader)
        {
            Description = "Interface for accessing network elements (nodes and line segments)";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");
        }
    }
}
