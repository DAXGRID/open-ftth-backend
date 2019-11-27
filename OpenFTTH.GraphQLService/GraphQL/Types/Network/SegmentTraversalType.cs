using ConduitNetwork.ReadModel;
using Core.ReadModel.Network;
using GraphQL.DataLoader;
using GraphQL.Types;
using QueryModel.Conduit;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class SegmentTraversalType : ObjectGraphType<SegmentTraversalInfo>
    {
        public SegmentTraversalType(IDataLoaderContextAccessor dataLoader)
        {
            Description = "Utility interface to do various traversals on segment level.";

            Field(x => x.StartRouteNode, type: typeof(RouteNodeType)).Description("Get start route node by doing an upstream segment trace.");
            Field(x => x.EndRouteNode, type: typeof(RouteNodeType)).Description("Get end route node by doing an downstream segment trace.");
            Field(x => x.StartRouteSegment, type: typeof(RouteSegmentType)).Description("Get start route segment by doing an upstream segment trace.");
            Field(x => x.EndRouteSegment, type: typeof(RouteSegmentType)).Description("Get end route segment by doing an downstream segment trace.");
            Field(x => x.AllRouteNodes, type: typeof(ListGraphType<RouteNodeType>)).Description("Get all route nodes doing a traversal from the segment at hand.");
            Field(x => x.AllRouteSegments, type: typeof(ListGraphType<RouteNodeType>)).Description("Get all route segments doing a traversal from the segment at hand.");
            Field(x => x.AllSegments, type: typeof(ListGraphType<SegmentInterface>)).Description("Get all equipment segments doing a traversal from the segment at hand.");
        }
    }
}
