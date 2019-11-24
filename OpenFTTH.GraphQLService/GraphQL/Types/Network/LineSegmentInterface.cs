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

            Field(x => x.Line.LineKind, type: typeof(LineSegmentKindType)).Description("Type of line segment - i.e. conduit, power cable, signal cable etc.");
        }
    }
}
