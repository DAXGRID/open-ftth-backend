using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
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
    public class ConduitInfoType : ObjectGraphType<ConduitInfo>
    {
        IRouteNetworkState routeNetworkQueryService;
        IConduitNetworkQueryService conduitNetworkEqueryService;

        public ConduitInfoType(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IDataLoaderContextAccessor dataLoader)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkEqueryService = conduitNetworkEqueryService;

            Description = "A conduit. Can be a multi conduit (i.e. has inner ducts) or a single conduit.";

            // Interface fields

            Interface<LineInterface>();

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");

            Field(x => x.LineKind, type: typeof(LineSegmentKindType)).Description("Type of line - i.e. multi conduit, single conduit, fiber cable etc.");

            Field(x => x.FromRouteNode, type: typeof(NodeInterface)).Description("The node where this line equipment starts.");

            Field(x => x.ToRouteNode, type: typeof(NodeInterface)).Description("The node where this line equipment ends.");

            Field(x => x.Parent, type: typeof(LineInterface)).Description("The parent, if this object is part of a composite equipment structure - i.e. a fiber inside a fiber cable or an inner conduit inside a multi conduit. Notice that the parent-child relationship on line level only cover the relationship inside a single composite equipment such as a fiber cable or multi conduit. Containment relationships between different types of equipment is on segment level only.");

            /*
            Field<IdGraphType>(
"Parent",
"The parent, if this object is part of a composite equipment structure - i.e. a fiber inside a fiber cable or an inner conduit inside a multi conduit. Notice that the parent-child relationship on line level only cover the relationship inside a single composite equipment such as a fiber cable or multi conduit. Containment relationships between different types of equipment is on segment level only.",
resolve: context =>
{
 return context.Source.Parent;
});
*/

            // Additional fields, some for backwards compabtiblely

            Field<ConduitKindEnumType>("Kind", "Kind of conduit (multi or single conduit)");
            Field(x => x.Name, type: typeof(IdGraphType)).Description("The uility might give each conduit a name/number");

            Field<IdGraphType>(
            "Position",
            "The position of the conduit inside a multi conduit. Field only populated on inner conduits (conduits inside a multi conduit)",
            resolve: context =>
            {
                return context.Source.SequenceNumber;
            });

            //Field(x => x.SequenceNumber, type: typeof(IdGraphType)).Description("The position of the conduit inside a multi conduit. Field only populated on inner conduits (conduits inside a multi conduit)");


            Field<ConduitShapeKindEnumType>("Shape", "Shape of conduit - flat, round etc.");
            Field<ConduitColorEnumType>("Color", "Color of the conduit itself");
            Field<ConduitColorEnumType>("ColorMarking", "Normally a colored stripe to distinguish between many conduits of same type in a trench");
            Field(x => x.TextMarking, type: typeof(IdGraphType)).Description("Normally some text printed along the conduitto distinguish between many conduits of same type in a trench");
            Field(x => x.InnerDiameter, type: typeof(IdGraphType)).Description("Inner diameter of the conduit");
            Field(x => x.OuterDiameter, type: typeof(IdGraphType)).Description("Outer diameter of the conduit");
            Field(x => x.AssetInfo, type: typeof(AssetInfoType)).Description("Asset info");
            Field(x => x.Children, type: typeof(ListGraphType<ConduitInfoType>)).Description("Child conduits. Field only populated on multi conduits.");

            //Field(x => x.Parent, type: typeof(ConduitInfoType)).Description("The parent of an inner conduit. Not available on multi and single conduits.");
            
            /*
            Field<RouteNodeType>(
            "FromRouteNode",
            resolve: context =>
            {
                var woi = routeNetworkQueryService.GetWalkOfInterestInfo(context.Source.GetRootConduit().WalkOfInterestId);
                return routeNetworkQueryService.GetRouteNodeInfo(woi.StartNodeId);
            });

            Field<RouteNodeType>(
            "ToRouteNode",
            resolve: context =>
            {
                var woi = routeNetworkQueryService.GetWalkOfInterestInfo(context.Source.GetRootConduit().WalkOfInterestId);
                return routeNetworkQueryService.GetRouteNodeInfo(woi.EndNodeId);
            });
            */

            Field<ListGraphType<RouteSegmentType>>(
            "AllRouteSegments",
            resolve: context =>
            {
                List<RouteSegmentInfo> result = new List<RouteSegmentInfo>();

                var woi = routeNetworkQueryService.GetWalkOfInterestInfo(context.Source.GetRootConduit().WalkOfInterestId);

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

                var woi = routeNetworkQueryService.GetWalkOfInterestInfo(context.Source.GetRootConduit().WalkOfInterestId);

                foreach (var nodeId in woi.AllNodeIds)
                {
                    result.Add(routeNetworkQueryService.GetRouteNodeInfo(nodeId));
                }

                return result;
            });
        }
    }
}
