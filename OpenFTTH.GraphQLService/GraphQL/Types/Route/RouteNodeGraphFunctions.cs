using Asset.Model;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel;
using Core.GraphSupport.Model;
using Core.ReadModel.Network;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using QueryModel.Conduit;
using RouteNetwork.Events.Model;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class RouteNodeGraphFunctions : ObjectGraphType
    {
        IRouteNetworkState routeNetworkQueryService;
        IConduitNetworkQueryService conduitNetworkEqueryService;
        IConduitClosureRepository conduitClosureRepository;

        public RouteNodeGraphFunctions(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IConduitClosureRepository conduitClosureRepository, IDataLoaderContextAccessor dataLoader)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkEqueryService = conduitNetworkEqueryService;
            this.conduitClosureRepository = conduitClosureRepository;

            Description = "Route node graph functions";

            Field<ListGraphType<IdGraphType>>(
              "shortestPath",
              arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "ToNodeId" }),
              resolve: context =>
              {
                  var startNode = context.Source as RouteNodeInfo;

                  Guid endNodeId = context.GetArgument<Guid>("toNodeId");

                  List<Guid> result = new List<Guid>();

                  List<IGraphElement> graphElements = new List<IGraphElement>();

                  graphElements.AddRange(routeNetworkQueryService.GetAllRouteNodes());
                  graphElements.AddRange(routeNetworkQueryService.GetAllRouteSegments());

                  var graph = new Graph(graphElements);


                  var shortestPathResult = graph.ShortestPath(startNode.Id.ToString(), endNodeId.ToString()).Select(s => s.Id);

                  return shortestPathResult;
              });


            Field<ListGraphType<RouteNodeType>>(
               "neighborNodes",
               resolve: context =>
               {
                   List<RouteNodeInfo> result = new List<RouteNodeInfo>();

                   var routeNode = context.Source as RouteNodeInfo;

                   foreach (var neighbor in routeNode.NeighborElements)
                   {
                       var neighborNeighbors = neighbor.NeighborElements;

                       foreach (var neighborNeighbor in neighborNeighbors)
                       {
                           if (neighborNeighbor != routeNode)
                               result.Add((RouteNodeInfo)neighborNeighbor);
                       }
                       
                   }

                   return result;

               });


            Field<ListGraphType<RouteSegmentType>>(
              "neighborSegments",
              resolve: context =>
              {
                  List<RouteSegmentInfo> result = new List<RouteSegmentInfo>();

                  var routeNode = context.Source as RouteNodeInfo;

                  foreach (var neighbor in routeNode.NeighborElements)
                  {
                    result.Add((RouteSegmentInfo)neighbor);
                  }

                  return result;
              });

        }
    }
}
