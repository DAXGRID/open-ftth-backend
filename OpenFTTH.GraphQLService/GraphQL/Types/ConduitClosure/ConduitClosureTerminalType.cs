using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.ReadModel.ConduitClosure;
using GraphQL.DataLoader;
using GraphQL.Types;
using MediatR;
using Network.Trace;
using QueryModel.Conduit;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitClosureTerminalType : ObjectGraphType<ConduitClosureTerminalInfo>
    {
        IRouteNetworkState routeNetworkQueryService;
        IConduitNetworkQueryService conduitNetworkEqueryService;

        public ConduitClosureTerminalType(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IDataLoaderContextAccessor dataLoader)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkEqueryService = conduitNetworkQueryService;

            var traversal = new TraversalHelper(routeNetworkQueryService);

            Description = "A specific terminal part of a condult closure port. Terminal are like sides and ports numbered clockwise reflecting their position. Only single conduits or cables can be attached to terminals.";

            Field(x => x.Position, type: typeof(IntGraphType)).Description("Terminal position/number.");

            Field(x => x.LineSegment, type: typeof(ConduitSegmentType)).Description("The single conduit or cable attacthed to the terminal.");

            Field(x => x.ConnectionKind, type: typeof(ConduitClosureInternalConnectionKindType)).Description("The type of connection the cable or single conduit segment has (or don't have) to another port terminal in the closure.");

            Field(x => x.ConnectedToSide, type: typeof(ConduitClosureSideEnumType)).Description("The other end side position/number, if the cable/conduit segment is connected to another cable/conduit segment (attached to another port terminal) or passing through to another port terminal.");

            Field(x => x.ConnectedToPort, type: typeof(IntGraphType)).Description("The other end port position/number, if the cable/conduit segment is connected to another cable/conduit segment (attached to another port terminal) or passing through to another port terminal.");

            Field(x => x.ConnectedToTerminal, type: typeof(IntGraphType)).Description("The other end terminal position/number, if the cable/conduit segment is connected to another cable/conduit segment (attached to another port terminal) or passing through to another port terminal.");

            Field<StringGraphType>(
             "DiagramLabel",
             resolve: context =>
             {
                 // If conduit segment, show end node name
                 if (context.Source.LineSegment != null && context.Source.LineSegment is ConduitSegmentInfo)
                 {
                     var conduitSegmentInfo = context.Source.LineSegment as ConduitSegmentInfo;
                     var lineInfo = traversal.CreateTraversalInfoFromSegment(conduitSegmentInfo);

                     if (context.Source.LineSegmentEndKind == ConduitEndKindEnum.Incomming)
                         return lineInfo.StartRouteNode.Name;
                     else
                         return lineInfo.EndRouteNode.Name;
                 }


                 return null;
             });

        }
    }
}
