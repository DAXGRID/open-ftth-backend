using Asset.Model;
using Core.ReadModel.Network;
using GraphQL.DataLoader;
using GraphQL.Types;
using QueryModel.Conduit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class SegmentInterface : InterfaceGraphType<ISegment>
    {
        public SegmentInterface(IDataLoaderContextAccessor dataLoader)
        {
            Name = "ISegment";

            Description = "Interface for accessing general line segment information.";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");

            Field(type: typeof(LineSegmentRelationTypeEnumType), "RelationType","Type of relation (incomming, outgoing, pass by or bass through) this segment has to the route element (route node or route segment) you have queried.");

            Field(x => x.Line.LineKind, type: typeof(LineSegmentKindType)).Description("Type of line segment - i.e. multi conduit, single conduit, fiber cable etc.");

            Field(x => x.Line, type: typeof(LineInterface)).Description("Line asset that this segment belongs to. The line normally represent some physical asset placed in the network - i.e. a conduit or fiber cable. Whereas the underlying segments are used to support connectivity when the original line asset is chopped up in pieces.");
               
            Field(x => x.Parents, type: typeof(ListGraphType<SegmentInterface>)).Description("The parent segments of this segment. As an example, if this is a fiber cable, then it typically will be blown through many inner conduit segments.");

            Field(x => x.Children, type: typeof(ListGraphType<SegmentInterface>)).Description("The child segments of this segment. As an example, if this is multi conduit, then child segments might be fiber cable segments or inner conduit segments running inside the multi conduit.");

            Field(x => x.FromRouteNode, type: typeof(NodeInterface)).Description("The node where this segment starts. Notice that it can be different from where the line starts if the line asset has been chopped up / breaked out.");

            Field(x => x.ToRouteNode, type: typeof(NodeInterface)).Description("The node where this segment ends. Notice that it can be different from where the line ends if the line asset has been chopped up / breaked out.");
        }
    }
}
