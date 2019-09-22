using EquipmentService.GraphQL.Types;
using GraphQL;
using GraphQL.Types;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Queries
{
    public class EquipmentServiceQuery : ObjectGraphType
    {
        public EquipmentServiceQuery(IRouteNetworkQueryService routeNetwork)
        {
            Description = "GraphQL API for querying OpenFTTH data";
            
            Field<StringGraphType>("apiVersion", resolve: context => VersionInfo.VersionString());

            Field<ListGraphType<RouteNodeType>>(
                "routeNodes",
                resolve: context => 
                {
                    return routeNetwork.GetAllRouteNodes();
                }
            );

            Field<RouteNodeType>(
                "routeNode",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "Id" }),
                resolve: context => 
                {
                    Guid id;
                    if (!Guid.TryParse(context.GetArgument<string>("id"), out id))
                    {
                        context.Errors.Add(new ExecutionError("Wrong value for guid"));
                        return null;
                    }
                    return routeNetwork.GetRouteNodeInfo(id);
                }   
            );


            Field<ListGraphType<RouteSegmentType>>(
                "routeSegments",
                resolve: context => {
                    return routeNetwork.GetAllRouteSegments();
                }
            );

            Field<RouteSegmentType>(
               "routeSegment",
               arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "Id" }),
               resolve: context =>
               {
                   Guid id;
                   if (!Guid.TryParse(context.GetArgument<string>("id"), out id))
                   {
                       context.Errors.Add(new ExecutionError("Wrong value for guid"));
                       return null;
                   }
                   return routeNetwork.GetRouteSegmentInfo(id);
               }
           );

           Field<ConduitServiceQuery>("conduitService", resolve: context => new { });
        }
    }
}
