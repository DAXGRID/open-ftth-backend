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

    public class LineInterface : InterfaceGraphType<ILine>
    {
        public LineInterface(IDataLoaderContextAccessor dataLoader)
        {
            Name = "ILine";

            Description = "Interface for accessing general line information. A line equals a physical asset forming an open or closed polyline such as a fiber cable, fiber, conduit etc. ";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");

            Field(x => x.LineKind, type: typeof(LineSegmentKindType)).Description("Type of line - i.e. multi conduit, single conduit, fiber cable etc.");

            Field(x => x.FromRouteNode, type: typeof(NodeInterface)).Description("The node where this line equipment starts.");

            Field(x => x.ToRouteNode, type: typeof(NodeInterface)).Description("The node where this line equipment ends.");

            Field(x => x.Parent, type: typeof(LineInterface)).Description("The parent, if this object is part of a composite equipment structure - i.e. a fiber cable holding a fiber, or multi conduit holding a inner conduit. Notice that the parent-child relationship on line level only cover the relationship inside a composite equipment such as a fiber cable or multi conduit. Containment relationships between different types of equipment is on segment level only.");

            Field(x => x.Children, type: typeof(ListGraphType<LineInterface>)).Description("The children of a composite equipment structure - i.e. inner conduits part of a multi conduit. Notice that the parent-child relationship on line level only cover the relationship inside a composite equipment such as a fiber cable or multi conduit. Containment relationships between different types of equipment is on segment level only.");
        }
    }
   
}
