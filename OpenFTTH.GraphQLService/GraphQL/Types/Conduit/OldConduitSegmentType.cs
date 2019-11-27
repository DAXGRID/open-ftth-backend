using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using GraphQL.DataLoader;
using GraphQL.Types;
using Network.Trace;
using QueryModel.Conduit;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitSegmentType : ObjectGraphType<ConduitSegmentInfo>
    {
        IRouteNetworkState routeNetworkQueryService;
        IConduitNetworkQueryService conduitNetworkEqueryService;

        public ConduitSegmentType(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IDataLoaderContextAccessor dataLoader)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkEqueryService = conduitNetworkEqueryService;

            var traversal = new TraversalHelper(routeNetworkQueryService);

            Description = "A conduit segment will initially be the original whole length piece of conduit. When the user starts to cut the conduit at various nodes, more conduit segments will emerge. Graph connectivity is maintained on segment level.";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");

            Field(x => x.Conduit, type:typeof(ConduitInfoType)).Description("The original conduit that this segment is part of.");

            Field(x => x.Children, type: typeof(ListGraphType<ConduitSegmentType>)).Description("The children of a multi conduit segment.");
            Field(x => x.Parents, type: typeof(ListGraphType<ConduitSegmentType>)).Description("The parents of an inner conduit segment.");

            Field<SegmentTraversalType>(
            "Line",
            resolve: context =>
            {
                return traversal.CreateTraversalInfoFromSegment(context.Source);
            });

            Field<RouteNodeType>(
            "FromRouteNode",
            resolve: context =>
            {
                return routeNetworkQueryService.GetRouteNodeInfo(context.Source.FromRouteNodeId);
            });

            Field<RouteNodeType>(
            "ToRouteNode",
            resolve: context =>
            {
                return routeNetworkQueryService.GetRouteNodeInfo(context.Source.ToRouteNodeId);
            });

            Field<ListGraphType<RouteSegmentType>>(
           "AllRouteSegments",
           resolve: context =>
           {
               List<RouteSegmentInfo> result = new List<RouteSegmentInfo>();

               var woi = routeNetworkQueryService.GetWalkOfInterestInfo(context.Source.Conduit.GetRootConduit().WalkOfInterestId).SubWalk2(context.Source.FromRouteNodeId, context.Source.ToRouteNodeId);

               foreach (var segmentId in woi.AllSegmentIds)
               {
                   result.Add(routeNetworkQueryService.GetRouteSegmentInfo(segmentId));
               }

               return result;
           });

            Field<ListGraphType<RouteNodeType>>(
            "AllRouteNodes",
            resolve: context =>
            {
                List<RouteNodeInfo> result = new List<RouteNodeInfo>();

                var woi = routeNetworkQueryService.GetWalkOfInterestInfo(context.Source.Conduit.GetRootConduit().WalkOfInterestId).SubWalk2(context.Source.FromRouteNodeId, context.Source.ToRouteNodeId);

                foreach (var nodeId in woi.AllNodeIds)
                {
                    result.Add(routeNetworkQueryService.GetRouteNodeInfo(nodeId));
                }

                return result;
            });

            Field(x => x.Line.LineKind, type: typeof(LineSegmentKindType)).Description("Type of line segment - i.e. conduit, power cable, signal cable etc.");


            //Interface<LineSegmentInterface>();
        }
    }
}
