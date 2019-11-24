using ConduitNetwork.Events.Model;
using Core.ReadModel.Network;
using GraphQL.DataLoader;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class LineSegmentKindType : EnumerationGraphType<LineKindEnum>
    {
        public LineSegmentKindType()
        {
            Name = "LineSegmentKind";
            Description = @"The kind of line segment - i.e. conduit, power cable, signal cable etc.";
        }
    }
}
