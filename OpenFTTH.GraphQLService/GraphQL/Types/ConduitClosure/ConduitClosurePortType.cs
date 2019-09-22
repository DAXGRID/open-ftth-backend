using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.ReadModel.ConduitClosure;
using GraphQL.DataLoader;
using GraphQL.Types;
using MediatR;
using QueryModel.Conduit;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitClosurePortType : ObjectGraphType<ConduitClosurePortInfo>
    {
        IRouteNetworkQueryService routeNetworkQueryService;
        IConduitNetworkQueryService conduitNetworkEqueryService;

        public ConduitClosurePortType(IRouteNetworkQueryService routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IDataLoaderContextAccessor dataLoader)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkEqueryService = conduitNetworkEqueryService;

            Description = "A specific port sitting on a side of a condult closure. Ports are like sides numbered clockwise reflecting their position. Only multi conduits can be attached to ports. A port might have no multi conduit attached, in the case were single conduits or cables are connected to the closure port terminals without being part of (running inside) a multi conduit.";

            Field(x => x.DiagramLabel, type: typeof(StringGraphType)).Description("Label informaion the user like to be displayed on the port. Could e.g. be a the multi conduit model type.");

            Field(x => x.Position, type: typeof(IntGraphType)).Description("Port position/number.");

            Field(x => x.MultiConduitSegment, type: typeof(ConduitSegmentType)).Description("The multi conduit segment attached to the port. Null of no multi conduit is used.");

            Field(x => x.ConnectionKind, type: typeof(ConduitClosureInternalConnectionKindType)).Description("The type of connection the multi conduit segment has (or don't have) to another port in the closure.");

            Field(x => x.ConnectedToSide, type: typeof(ConduitClosureSideEnumType)).Description("The other end side position/number, if the multi conduit segment is connected to another multi conduit segment (attached to another port) or passing through to another port.");

            Field(x => x.ConnectedToPort, type: typeof(IntGraphType)).Description("The other end port position/number, if the multi conduit segment is connected to another multi conduit segment (attached to another port) or passing through to another port.");

            Field(x => x.Terminals, type: typeof(ListGraphType<ConduitClosureTerminalType>)).Description("Terminals of the port.");

        }
    }
}
