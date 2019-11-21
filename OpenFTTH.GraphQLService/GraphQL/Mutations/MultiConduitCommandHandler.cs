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
    public class MultiConduitCommandHandler : ObjectGraphType
    {
        public MultiConduitCommandHandler(IMediator commandBus, IConduitClosureRepository conduitClosureRepository, IRouteNetworkState routeNetwork, IConduitNetworkQueryService conduitNetwork)
        {
            
            Description = "API for sending commands to the multi conduit aggregate root";

            Field<ConduitInfoType>(
              "cutOuterConduitCommand",
              description: "Cut the outer conduit of a multi conduit",
              arguments: new QueryArguments(
                  new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "pointOfInterestId" },
                  new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "multiConduitId" }
              ),
              resolve: context =>
              {
                  var cutOuterConduitCmd = new CutOuterConduitCommand()
                  {
                      MultiConduitId = context.GetArgument<Guid>("multiConduitId"),
                      PointOfInterestId = context.GetArgument<Guid>("pointOfInterestId")
                  };

                  try
                  {
                      commandBus.Send(cutOuterConduitCmd).Wait();
                  }
                  catch (Exception ex)
                  {
                      context.Errors.Add(new ExecutionError(ex.Message, ex));
                      return null;
                  }

                  return conduitNetwork.GetMultiConduitInfo(cutOuterConduitCmd.MultiConduitId);
              }
          );

            Field<ConduitInfoType>(
              "cutInnerConduitCommand",
              description: "Cut the outer conduit of a multi conduit",
              arguments: new QueryArguments(
                  new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "pointOfInterestId" },
                  new QueryArgument<IdGraphType> { Name = "innerConduitId" },
                  new QueryArgument<IdGraphType> { Name = "multiConduitId" },
                  new QueryArgument<IntGraphType> { Name = "innerConduitNumber" }
              ),
              resolve: context =>
              {
                  var cutInnerConduitCmd = new CutInnerConduitCommand()
                  {
                      MultiConduitId = context.GetArgument<Guid>("multiConduitId"),
                      InnerConduitId = context.GetArgument<Guid>("innerConduitId"),
                      PointOfInterestId = context.GetArgument<Guid>("pointOfInterestId"),
                      InnerConduitSequenceNumber = context.GetArgument<int>("innerConduitNumber")
                  };

                  try
                  {
                      commandBus.Send(cutInnerConduitCmd).Wait();
                  }
                  catch (Exception ex)
                  {
                      context.Errors.Add(new ExecutionError(ex.Message, ex));
                      return null;
                  }

                  return conduitNetwork.GetMultiConduitInfo(cutInnerConduitCmd.MultiConduitId);
              }
          );


            Field<ConduitInfoType>(
              "connectInnerConduitToJunction",
              description: "Connect an inner conduit of the multi conduit to a junction (single conduit connector). The junction/connector will be created automatically.",
              arguments: new QueryArguments(
                  new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "pointOfInterestId" },
                  new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "multiConduitId" },
                  new QueryArgument<NonNullGraphType<ConduitEndKindEnumType>> { Name = "conduitEndKind" },
                  new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "innerConduitNumber" },
                  new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "junctionConnectorId" }
              ),
              resolve: context =>
              {
                  var connectToJunction = new ConnectInnerConduitToJunction()
                  {
                      MultiConduitId = context.GetArgument<Guid>("multiConduitId"),
                      ConnectedEndKind = context.GetArgument<ConduitEndKindEnum>("conduitEndKind"),
                      PointOfInterestId = context.GetArgument<Guid>("pointOfInterestId"),
                      InnerConduitSequenceNumber = context.GetArgument<int>("innerConduitNumber"),
                      ConnectedJunctionId = context.GetArgument<Guid>("junctionConnectorId")
                  };

                  try
                  {
                      commandBus.Send(connectToJunction).Wait();
                  }
                  catch (Exception ex)
                  {
                      context.Errors.Add(new ExecutionError(ex.Message, ex));
                      return null;
                  }

                  return conduitNetwork.GetMultiConduitInfo(connectToJunction.MultiConduitId);
              }
          );


            Field<ConduitInfoType>(
             "connectConduitSegmentToConduitSegment",
             description: "A convenience function for connecting conduits together. It will automatically create junctions. Also, if the to conduit segment is a multi conduit, the function will automatically create an inner conduit in the to multi conduit of same type as the from single conduit.",
             arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "pointOfInterestId" },
                 new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "fromConduitSegmentId" },
                 new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "toConduitSegmentId" }
             ),
             resolve: context =>
             {
                 var connectConduitSegmentCmd = new ConnectConduitSegmentCommand()
                 {
                     PointOfInterestId = context.GetArgument<Guid>("pointOfInterestId"),
                     FromConduitSegmentId = context.GetArgument<Guid>("fromConduitSegmentId"),
                     ToConduitSegmentId = context.GetArgument<Guid>("toConduitSegmentId")
                 };

                 try
                 {
                     commandBus.Send(connectConduitSegmentCmd).Wait();
                 }
                 catch (Exception ex)
                 {
                     context.Errors.Add(new ExecutionError(ex.Message, ex));
                     return null;
                 }

                 return null;
             }
         );





        }
    }
}
