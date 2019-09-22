using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.ReadModel.ConduitClosure;
using GraphQL.DataLoader;
using GraphQL.Types;
using MediatR;
using QueryModel.Conduit;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitClosureSideType : ObjectGraphType<ConduitClosureSideInfo>
    {
        IRouteNetworkQueryService routeNetworkQueryService;
        IConduitNetworkQueryService conduitNetworkEqueryService;

        public ConduitClosureSideType(IRouteNetworkQueryService routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IDataLoaderContextAccessor dataLoader)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkEqueryService = conduitNetworkEqueryService;

            Description = "A specific side of a conduit closure. Sides are numbered clockwise: 1=Top 2=Right 3=Buttom 4=Left";

            Field(x => x.DigramLabel, type:typeof(StringGraphType)).Description("Label the user/system like to be placed along the side. Could e.g. be a street name.");
            Field(x => x.Position, type: typeof(ConduitClosureSideEnumType)).Description("Side position. 1=Top 2=Right 3=Buttom 4=Left");
            Field(x => x.Ports, type: typeof(ListGraphType<ConduitClosurePortType>)).Description("Ports on the side");

        }
    }
}
