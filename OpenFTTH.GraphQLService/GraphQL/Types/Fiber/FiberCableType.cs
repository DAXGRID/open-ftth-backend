using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using FiberNetwork.Events.Model;
using GraphQL.DataLoader;
using GraphQL.Types;
using QueryModel.Conduit;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class FiberCableType : ObjectGraphType<FiberCableInfo>
    {
        IRouteNetworkState routeNetworkQueryService;
        IConduitNetworkQueryService conduitNetworkEqueryService;

        public FiberCableType(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IDataLoaderContextAccessor dataLoader)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkEqueryService = conduitNetworkEqueryService;

            Description = "A fiber cable.";

            // Interface fields

            Interface<LineInterface>();

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");

            Field(x => x.LineKind, type: typeof(LineSegmentKindType)).Description("Type of line - i.e. multi conduit, single conduit, fiber cable etc.");

            Field(x => x.FromRouteNode, type: typeof(NodeInterface)).Description("The node where this line equipment starts.");

            Field(x => x.ToRouteNode, type: typeof(NodeInterface)).Description("The node where this line equipment ends.");

            Field(x => x.Parent, type: typeof(LineInterface)).Description("The parent, if this object is part of a composite equipment structure - i.e. a fiber inside a fiber cable or an inner conduit inside a multi conduit. Notice that the parent-child relationship on line level only cover the relationship inside a single composite equipment such as a fiber cable or multi conduit. Containment relationships between different types of equipment is on segment level only.");


            // Fiber cable specific fields

            Field(x => x.Name, type: typeof(IdGraphType)).Description("The uility might give each conduit a name/number");

            Field(x => x.SequenceNumber, type: typeof(IdGraphType)).Description("The position of fiber cable inside of conduit or route.");

            //Field(x => x.AssetInfo, type: typeof(AssetInfoType)).Description("Asset info");
         
            Field<IntGraphType>(
              "NumberOfFibers",
              resolve: context =>
              {
                  return context.Source.Children.Count();
              });

            /*
            Field<RouteNodeType>(
            "FromRouteNode",
            resolve: context =>
            {
                var woi = routeNetworkQueryService.GetWalkOfInterestInfo(context.Source.GetRoot().WalkOfInterestId);
                return routeNetworkQueryService.GetRouteNodeInfo(woi.StartNodeId);
            });

            Field<RouteNodeType>(
            "ToRouteNode",
            resolve: context =>
            {
                var woi = routeNetworkQueryService.GetWalkOfInterestInfo(context.Source.GetRoot().WalkOfInterestId);
                return routeNetworkQueryService.GetRouteNodeInfo(woi.EndNodeId);
            });
            */

            Field<ListGraphType<RouteSegmentType>>(
            "AllRouteSegments",
            resolve: context =>
            {
                List<RouteSegmentInfo> result = new List<RouteSegmentInfo>();

                var woi = routeNetworkQueryService.GetWalkOfInterestInfo(context.Source.GetRoot().WalkOfInterestId);

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

                var woi = routeNetworkQueryService.GetWalkOfInterestInfo(context.Source.GetRoot().WalkOfInterestId);

                foreach (var nodeId in woi.AllNodeIds)
                {
                    result.Add(routeNetworkQueryService.GetRouteNodeInfo(nodeId));
                }

                return result;
            });
        }
    }
}
