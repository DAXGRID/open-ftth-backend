using ConduitNetwork.Business.Specifications;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using DiagramLayout.Builder;
using DiagramLayout.Builder.Mockup;
using DiagramLayout.IO;
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
        public DiagramServiceQuery(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IConduitClosureRepository conduitClosureRepository, IDataLoaderContextAccessor dataLoader)
        {
            Description = "GraphQL API for generating fiber equiment diagrams.";

            Field<DiagramType>(
                "buildRouteNodeDiagram",
                 arguments: new QueryArguments(
                  new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "routeNodeId" },
                  new QueryArgument<StringGraphType> { Name = "exportGeojsonFileName" }
                  ),
                resolve: context => 
                {
                    var routeNodeId = context.GetArgument<Guid>("routeNodeId");
                    var jsonFilename = context.GetArgument<string>("exportGeojsonFileName");

                    var diagram = new ConduitClosureBuilder().Build(routeNodeId, routeNetworkQueryService, conduitNetworkEqueryService, conduitClosureRepository);

                    if (jsonFilename != null)
                    {
                        var export = new GeoJsonExporter(diagram);
                        export.Export(jsonFilename);
                    }

                    return diagram;
                }
            );
        }
    }
}
