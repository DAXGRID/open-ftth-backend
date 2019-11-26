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

            Field(x => x.Line, type: typeof(LineInterface)).Description("Line that this segment belongs to.");

            Field(x => x.Line.LineKind, type: typeof(LineSegmentKindType)).Description("Type of line segment - i.e. multi conduit, single conduit, fiber cable etc.");

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");

            Field(x => x.Parents, type: typeof(ListGraphType<LineSegmentInterface>)).Description("The parent segments of this segment, if this segment is contained within another segment network - i.e. a fiber cable segment running within one of more conduit segments.");
        }
    }
}
