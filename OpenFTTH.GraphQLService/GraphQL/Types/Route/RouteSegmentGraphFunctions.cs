using Asset.Model;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel;
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
    public class RouteSegmentGraphFunctions : ObjectGraphType
    {
        IRouteNetworkState routeNetworkQueryService;
        IConduitNetworkQueryService conduitNetworkEqueryService;
        IConduitClosureRepository conduitClosureRepository;

        public RouteSegmentGraphFunctions(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IConduitClosureRepository conduitClosureRepository, IDataLoaderContextAccessor dataLoader)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkEqueryService = conduitNetworkEqueryService;
            this.conduitClosureRepository = conduitClosureRepository;

            Description = "Route segment graph functions";

            Field<ListGraphType<RouteNodeType>>(
               "neighborNodes",
               resolve: context =>
               {
                   List<RouteNodeInfo> result = new List<RouteNodeInfo>();

                   var routeSegment = context.Source as RouteSegmentInfo;

                   foreach (var neighbor in routeSegment.NeighborElements)
                   {
                    result.Add((RouteNodeInfo)neighbor);
                   }

                   return result;

               });


            Field<ListGraphType<RouteSegmentType>>(
               "neighborSegments",
               resolve: context =>
               {
                   List<RouteSegmentInfo> result = new List<RouteSegmentInfo>();

                   var routeSegment = context.Source as RouteSegmentInfo;

                   foreach (var neighbor in routeSegment.NeighborElements)
                   {
                       var neighborNeighbors = neighbor.NeighborElements;

                       foreach (var neighborNeighbor in neighborNeighbors)
                       {
                           if (neighborNeighbor != routeSegment)
                               result.Add((RouteSegmentInfo)neighborNeighbor);
                       }

                   }

                   return result;

               });

        }
    }
}
