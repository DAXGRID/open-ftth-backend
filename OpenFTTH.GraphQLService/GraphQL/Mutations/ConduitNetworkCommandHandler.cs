using Asset.Model;
using ConduitNetwork.Business.Commands;
using ConduitNetwork.Business.Specifications;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel.ConduitClosure;
using EquipmentService.GraphQL.Types;
using GraphQL;
using GraphQL.Types;
using MediatR;
using RouteNetwork.Business.Commands;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.ConduitClosure
{
    public class ConduitServiceCommandHandler : ObjectGraphType
    {
        public ConduitServiceCommandHandler(IMediator commandBus, IConduitClosureRepository conduitClosureRepository, IConduitNetworkQueryService conduitNetworkQueryService, IRouteNetworkState routeNetwork, IConduitNetworkQueryService conduitNetwork, IConduitSpecificationRepository conduitSpecificationRepository)
        {
            Description = "API for sending commands to the conduit service";

            Field<ConduitClosureCommandHandler>("conduitClosure", resolve: context => new { });
            Field<MultiConduitCommandHandler>("multiConduit", resolve: context => new { });
            Field<SingleConduitCommandHandler>("singleConduit", resolve: context => new { });

            Field<ConduitInfoType>(
             "placeConduit",
             description: "Place a multi or single conduit in the route network",
             arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "conduitSpecificationId" },
                 new QueryArgument<NonNullGraphType<ListGraphType<IdGraphType>>> { Name = "walkOfInterest", Description = "Route network walk specified as a list of route element ids (route-node-id, route-segment-id, route-node-id...)" },
                 new QueryArgument<IdGraphType> { Name = "conduitId", Description = "If not specified, a new guid will automatically be created" },
                 new QueryArgument<StringGraphType> { Name = "name" },
                 new QueryArgument<ConduitColorEnumType> { Name = "markingColor" },
                 new QueryArgument<StringGraphType> { Name = "markingText" }
             ),
             resolve: context =>
             {
                 try
                 {
                     var conduitSpec = conduitSpecificationRepository.GetConduitSpecification(context.GetArgument<Guid>("conduitSpecificationId"));

                     // Check that conduit not already exists


                     // First create walk of interest
                     var walkOfInterestCmd = new RegisterWalkOfInterestCommand()
                     {
                         WalkOfInterestId = Guid.NewGuid(),
                         RouteElementIds = context.GetArgument<List<Guid>>("walkOfInterest")

                     };

                     commandBus.Send(walkOfInterestCmd).Wait();


                     var conduitId = context.GetArgument<Guid>("conduitId");

                     // Multi conduit
                     if (conduitSpec.Kind == ConduitKindEnum.MultiConduit)
                     {
                         var placeConduitCommand = new PlaceMultiConduitCommand()
                         {
                             MultiConduitId = Guid.NewGuid(),
                             ConduitSpecificationId = context.GetArgument<Guid>("conduitSpecificationId"),
                             WalkOfInterestId = walkOfInterestCmd.WalkOfInterestId,
                             Name = context.GetArgument<string>("name"),
                             MarkingColor = context.GetArgument<ConduitColorEnum>("markingColor"),
                             MarkingText = context.GetArgument<string>("markingText")
                         };

                         if (conduitId != Guid.Empty)
                             placeConduitCommand.MultiConduitId = conduitId;

                         commandBus.Send(placeConduitCommand).Wait();

                         return conduitNetworkQueryService.GetMultiConduitInfo(placeConduitCommand.MultiConduitId);
                     }
                     // Single conduit
                     else
                     {
                         var placeConduitCommand = new PlaceSingleConduitCommand()
                         {
                             SingleConduitId = Guid.NewGuid(),
                             ConduitSpecificationId = context.GetArgument<Guid>("conduitSpecificationId"),
                             WalkOfInterestId = walkOfInterestCmd.WalkOfInterestId,
                             Name = context.GetArgument<string>("name"),
                             MarkingColor = context.GetArgument<ConduitColorEnum>("markingColor"),
                             MarkingText = context.GetArgument<string>("markingText")
                         };

                         if (conduitId != Guid.Empty)
                             placeConduitCommand.SingleConduitId = conduitId;

                         commandBus.Send(placeConduitCommand).Wait();

                         return conduitNetworkQueryService.GetSingleConduitInfo(placeConduitCommand.SingleConduitId);
                     }
                 }

                 catch (Exception ex)
                 {
                     context.Errors.Add(new ExecutionError(ex.Message, ex));
                 }

                 return null;
             
             });


             Field<StringGraphType>(
             "placeFiberCableWithinConduit",
             description: "Place a fiber cable inside a conduit",
             arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "cableSegmentId", Description = "Id of the cable segment to be placed inside a conduit" },
                 new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "conduitSegmentId1", Description = "Id of the conduit segment where the cable should be placed" },
                 new QueryArgument<IdGraphType> { Name = "conduitSegmentId2", Description = "Used when placing cables into a conduit that is cut and not connected in a well. The you must specify both the incomming and outgoing conduit segment in the well, because otherwise the cable has an unknown route." }
             ),
             resolve: context =>
             {
                 return null;
             });
        }
    }
}
