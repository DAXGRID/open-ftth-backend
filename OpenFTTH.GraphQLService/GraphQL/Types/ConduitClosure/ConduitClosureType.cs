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
    public class ConduitClosureType : ObjectGraphType<ConduitClosureInfo>
    {
        IRouteNetworkState routeNetworkQueryService;
        IConduitNetworkQueryService conduitNetworkEqueryService;

        public ConduitClosureType(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IDataLoaderContextAccessor dataLoader)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkEqueryService = conduitNetworkEqueryService;

            Description = "A conduit closure.";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");
            Field(x => x.Sides, type: typeof(ListGraphType<ConduitClosureSideType>)).Description("Sides of the closure");
        }
    }
}
