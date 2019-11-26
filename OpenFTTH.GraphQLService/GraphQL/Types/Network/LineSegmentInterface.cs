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
    public class LineSegmentInterface : InterfaceGraphType<ILineSegment>
    {
        public LineSegmentInterface(IDataLoaderContextAccessor dataLoader)
        {
            Description = "Interface for accessing general line segment information.";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");

            Field(type: typeof(LineSegmentRelationTypeEnumType), "RelationType","Type of relation (incomming, outgoing, pass by or bass through) this segment has to the node queried.");

            Field(x => x.Line.LineKind, type: typeof(LineSegmentKindType)).Description("Type of line segment - i.e. multi conduit, single conduit, fiber cable etc.");

            Field(x => x.Line, type: typeof(LineInterface)).Description("Line that this segment belongs to.");
               
            Field(x => x.Parents, type: typeof(ListGraphType<LineSegmentInterface>)).Description("The parent segments of this segment. As an example, if this is a fiber cable, then it typically will be blown through many inner conduit segments.");

            Field(x => x.Children, type: typeof(ListGraphType<LineSegmentInterface>)).Description("The child segments of this segment. As an example, if this is multi conduit, then child segments might be fiber cable segments or inner conduit segments running inside the multi conduit.");
        }
    }
}
