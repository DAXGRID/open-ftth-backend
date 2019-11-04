using Asset.Model;
using ConduitNetwork.Business.Commands;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel.ConduitClosure;
using EquipmentService.GraphQL.Types;
using GraphQL;
using GraphQL.Types;
using MediatR;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.ConduitClosure
{
    public class ConduitClosureCommandHandler : ObjectGraphType
    {
        public ConduitClosureCommandHandler(IMediator commandBus, IConduitClosureRepository conduitClosureRepository, IRouteNetworkState routeNetwork, IConduitNetworkQueryService conduitNetwork)
        {
            Description = "API for sending commands to the conduit closure aggregate root";

            Field<ConduitClosureType>(
                "placeConduitClosure",
                description: "Place a new conduit clousure in a point of interest (route node)",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "pointOfInterestId" },
                    new QueryArgument<IdGraphType> { Name = "conduitClosureId" }
                ),
                resolve: context =>
                {
                    var conduitClosureId = context.GetArgument<Guid>("conduitClosureId");

                    var createConduitClosureCmd = new PlaceConduitClosureCommand()
                    {
                        ConduitClosureId = Guid.NewGuid(),
                        PointOfInterestId = context.GetArgument<Guid>("pointOfInterestId")
                    };

                    if (conduitClosureId != Guid.Empty)
                        createConduitClosureCmd.ConduitClosureId = conduitClosureId;

                    try
                    {
                        commandBus.Send(createConduitClosureCmd).Wait();
                    }
                    catch (Exception ex)
                    {
                        context.Errors.Add(new ExecutionError(ex.Message, ex));
                        return null;
                    }

                    return conduitClosureRepository.GetConduitClosureInfo(createConduitClosureCmd.ConduitClosureId);
                }
            );

            Field<ConduitClosureType>(
                "removeConduitClosure",
                description: "Remove a new conduit clousure from its point of interest (route node)",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "conduitClosureId" }
                ),
                resolve: context =>
                {
                    var removeConduitClosureCmd = new RemoveConduitClosureCommand()
                    {
                        ConduitClosureId = context.GetArgument<Guid>("conduitClosureId")
                    };

                    try
                    {
                        commandBus.Send(removeConduitClosureCmd).Wait();
                    }
                    catch (Exception ex)
                    {
                        context.Errors.Add(new ExecutionError(ex.Message, ex));
                        return null;
                    }

                    return null;
                }
            );

            Field<ConduitClosureType>(
               "attachPassByConduitToClosure",
               description: "Attach a conduit that is passing by to the conduit closure",
               arguments: new QueryArguments(
                   new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "conduitClosureId", Description = "Id of the conduit closure where the passing conduit should be attached to" },
                   new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "conduitId", Description = "Id of the conduit to be attached to the conduit closure" },
                   new QueryArgument<NonNullGraphType<ConduitClosureSideEnumType>> { Name = "incommingSide", Description = "The side where the conduit should enter the closure. This argument is mandatory." },
                   new QueryArgument<NonNullGraphType<ConduitClosureSideEnumType>> { Name = "outgoingSide", Description = "The side where the conduit should leav the closure. This argument is mandatory." },
                   new QueryArgument<IntGraphType> { Name = "incommingPortPosition", Description = "The position where the port should be placed. If not specified, the system will position the new port after eventually existing ports. If specified, and the port number references an existing occupied port, the system will move the existing and following ports one position, and place the new port on the specified position."},
                   new QueryArgument<IntGraphType> { Name = "outgoingPortPosition", Description = "The position where the port should be placed. If not specified, the system will position the new port after eventually existing ports. If specified, and the port number references an existing occupied port, the system will move the existing and following ports one position, and place the new port on the specified position." },
                   new QueryArgument<IntGraphType> { Name = "incommingTerminalPosition", Description = "The position where the new terminal should be placed. Only used when attaching single conduits to closures. If specified the single conduit will be attached to a new terminal with the specified position on an existing port. If this argument is specified, you must also specify the port argument." },
                   new QueryArgument<IntGraphType> { Name = "outgoingTerminalPosition", Description = "The position where the new terminal should be placed. Only used when attaching single conduits to closures. If specified the single conduit will be attached to a new terminal with the specified position on an existing port. If this argument is specified, you must also specify the port argument." }
               ),
               resolve: context =>
               {
                   var routeConduitThroughClosureCmd = new AttachPassByConduitToClosureCommand()
                   {
                       ConduitClosureId = context.GetArgument<Guid>("conduitClosureId"),
                       ConduitId = context.GetArgument<Guid>("conduitId"),
                       IncommingSide = context.GetArgument<ConduitClosureSideEnum>("incommingSide"),
                       OutgoingSide = context.GetArgument<ConduitClosureSideEnum>("outgoingSide"),
                       IncommingPortPosition = context.GetArgument<int>("incommingPortPosition"),
                       OutgoingPortPosition = context.GetArgument<int>("outgoingPortPosition"),
                       IncommingTerminalPosition = context.GetArgument<int>("incommingPortPosition"),
                       OutgoingTerminalPosition = context.GetArgument<int>("outgoingPortPosition"),
                   };

                   try
                   {
                       commandBus.Send(routeConduitThroughClosureCmd).Wait();
                   }
                   catch (Exception ex)
                   {
                       context.Errors.Add(new ExecutionError(ex.Message, ex));
                       return null;
                   }

                   return conduitClosureRepository.GetConduitClosureInfo(routeConduitThroughClosureCmd.ConduitClosureId);
               }
           );


            Field<ConduitClosureType>(
               "attachConduitEndToClosure",
               description: "Attach a conduit to the conduit closure, that is ending in the point of interest (node) where the conduit closure is also located.",
               arguments: new QueryArguments(
                   new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "conduitClosureId", Description = "Id of the ending conduit that should be attached to the conduit closure." },
                   new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "conduitId", Description = "Id of the conduit to be attached to the conduit closure" },
                   new QueryArgument<NonNullGraphType<ConduitClosureSideEnumType>> { Name = "side", Description = "The side where the conduit should enter the closure. This argument is mandatory." },
                   new QueryArgument<IntGraphType> { Name = "portPosition", Description = "The position where the port should be placed. If not specified, the system will position the new port after eventually existing ports. If specified, and the port number references an existing occupied port, the system will move the existing and following ports one position, and place the new port on the specified position." },
                   new QueryArgument<IntGraphType> { Name = "terminalPosition", Description = "The position where the new terminal should be placed. Only used when attaching single conduits to closures. If specified the single conduit will be attached to a new terminal with the specified position on an existing port. If this argument is specified, you must also specify the port argument." }
               ),
               resolve: context =>
               {
                   var attachConduitEndCmd = new AttachConduitEndToClosureCommand()
                   {
                       ConduitClosureId = context.GetArgument<Guid>("conduitClosureId"),
                       ConduitId = context.GetArgument<Guid>("conduitId"),
                       Side = context.GetArgument<ConduitClosureSideEnum>("side"),
                       PortPosition = context.GetArgument<int>("portPosition"),
                       TerminalPosition = context.GetArgument<int>("terminalPosition"),
                   };

                   try
                   {
                       commandBus.Send(attachConduitEndCmd).Wait();
                   }
                   catch (Exception ex)
                   {
                       context.Errors.Add(new ExecutionError(ex.Message, ex));
                       return null;
                   }

                   return conduitClosureRepository.GetConduitClosureInfo(attachConduitEndCmd.ConduitClosureId);
               }
            );

        }
    }
}
