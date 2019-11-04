using Asset.Model;
using ConduitNetwork.Business.Commands;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel.ConduitClosure;
using EquipmentService.GraphQL.Types;
using GraphQL;
using GraphQL.Types;
using Marten;
using MediatR;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.ConduitClosure
{
    public class SingleConduitCommandHandler : ObjectGraphType
    {
        public SingleConduitCommandHandler(IMediator commandBus, IConduitClosureRepository conduitClosureRepository, IRouteNetworkState routeNetwork, IConduitNetworkQueryService conduitNetwork)
        {
            
            Description = "API for sending commands to the single conduit aggregate root";

            Field<ConduitInfoType>(
              "connectSingleConduitToJunction",
              description: "Connect the single conduit to a junction (single conduit connector). The junction/connector will be created automatically.",
              arguments: new QueryArguments(
                  new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "pointOfInterestId" },
                  new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "singleConduitId" },
                  new QueryArgument<NonNullGraphType<ConduitEndKindEnumType>> { Name = "conduitEndKind" },
                  new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "junctionConnectorId" }
              ),
              resolve: context =>
              {
                  var connectToJunction = new ConnectSingleConduitToJunction()
                  {
                      SingleConduitId = context.GetArgument<Guid>("singleConduitId"),
                      ConnectedEndKind = context.GetArgument<ConduitEndKindEnum>("conduitEndKind"),
                      PointOfInterestId = context.GetArgument<Guid>("pointOfInterestId"),
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

                  return conduitNetwork.GetSingleConduitInfo(connectToJunction.SingleConduitId);
              }
          );


        }
    }
}
