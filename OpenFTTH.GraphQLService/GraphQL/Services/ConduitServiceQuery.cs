using ConduitNetwork.Business.Specifications;
using ConduitNetwork.QueryService;
using EquipmentService.GraphQL.Types;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Queries
{
    public class ConduitServiceQuery : ObjectGraphType
    {
        public ConduitServiceQuery(IRouteNetworkQueryService routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IConduitSpecificationRepository conduitSpecificationRepository, IDataLoaderContextAccessor dataLoader)
        {
            Description = "GraphQL API for querying the conduit service.";

            Field<ListGraphType<ConduitSpecificationType>>(
                "conduitSpecifications",
                resolve: context => 
                {
                    return conduitSpecificationRepository.GetConduitSpecifications();
                }
            );
        }
    }
}
