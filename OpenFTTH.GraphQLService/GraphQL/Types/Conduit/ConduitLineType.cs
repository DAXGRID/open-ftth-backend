using ConduitNetwork.ReadModel;
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
    public class ConduitLineType : ObjectGraphType<ConduitLineInfo>
    {
        public ConduitLineType(IDataLoaderContextAccessor dataLoader)
        {
            Description = "A line is container of all conduits that is connected to each other. Think of it as a metro line, where the stops are route nodes and the sections between the stops are conduit segments";

            Field(x => x.StartRouteNode, type: typeof(RouteNodeType)).Description("Node at the start of the line");
            Field(x => x.EndRouteNode, type: typeof(RouteNodeType)).Description("Node at the end of the line");
            Field(x => x.StartRouteSegment, type: typeof(RouteSegmentType)).Description("Segment at the start of the line");
            Field(x => x.EndRouteSegment, type: typeof(RouteSegmentType)).Description("Segment at the end of the line");
            Field(x => x.AllRouteNodes, type: typeof(ListGraphType<RouteNodeType>)).Description("All line nodes");
            Field(x => x.AllRouteSegments, type: typeof(ListGraphType<RouteNodeType>)).Description("All line segments");

            Field(x => x.AllConduitSegments, type: typeof(ListGraphType<ConduitSegmentType>)).Description("All conduit segments making up the line");


            /*
            Field<ConduitKindEnumType>("ConduitKind", "Kind of conduit (multi or single conduit)");
            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");
            Field(x => x.Name, type: typeof(IdGraphType)).Description("The uility might give each conduit a name/number");
            Field(x => x.Position, type: typeof(IdGraphType)).Description("The position of the conduit inside a multi conduit. Field only populated on inner conduits (conduits inside a multi conduit)");
            Field<ConduitShapeKindEnumType>("Shape", "Shape of conduit - flat, round etc.");
            Field<ConduitColorEnumType>("Color", "Color of the conduit itself");
            Field<ConduitColorEnumType>("ColorMarking", "Normally a colored stripe to distinguish between many conduits of same type in a trench");
            Field(x => x.TextMarking, type: typeof(IdGraphType)).Description("Normally some text printed along the conduitto distinguish between many conduits of same type in a trench");
            Field(x => x.InnerDiameter, type: typeof(IdGraphType)).Description("Inner diameter of the conduit");
            Field(x => x.OuterDiameter, type: typeof(IdGraphType)).Description("Outer diameter of the conduit");
            Field(x => x.AssetInfo, type: typeof(AssetInfoType)).Description("Asset info");
            Field(x => x.Children, type: typeof(ListGraphType<ConduitType>)).Description("Child conduits. Field only populated on multi conduits.");
            
                public RouteNodeInfo StartNode { get; set; }
        public RouteSegmentInfo StartSegment { get; set; }
        public RouteNodeInfo EndNode { get; set; }
        public RouteSegmentInfo EndSegment { get; set; }
        public List<RouteNodeInfo> AllNodes { get; set; }
        public List<RouteSegmentInfo> AllSegments { get; set; }
        public List<ConduitSegment> ConduitsSegments { get; set; }
        */
        }
    }
}
