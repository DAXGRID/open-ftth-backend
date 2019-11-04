using ConduitNetwork.Business.Specifications;
using ConduitNetwork.QueryService;
using DiagramLayout.Builder.Mockup;
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
    public class DiagramServiceQuery : ObjectGraphType
    {
        public DiagramServiceQuery(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IConduitSpecificationRepository conduitSpecificationRepository, IDataLoaderContextAccessor dataLoader)
        {
            Description = "GraphQL API for generating fiber equiment diagrams.";

            Field<ListGraphType<DiagramType>>(
                "buildDiagram",
                resolve: context => 
                {
                    return new MockupFlexpoint().Build();
                }
            );
        }
    }
}
