using Asset.Model;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using QueryModel.Conduit;
using RouteNetwork.Events.Model;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class RouteNodeType : ObjectGraphType<RouteNodeInfo>
    {
        IRouteNetworkQueryService routeNetworkQueryService;
        IConduitNetworkQueryService conduitNetworkEqueryService;
        IConduitClosureRepository conduitClosureRepository;

        public RouteNodeType(IRouteNetworkQueryService routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IConduitClosureRepository conduitClosureRepository, IDataLoaderContextAccessor dataLoader)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkEqueryService = conduitNetworkEqueryService;
            this.conduitClosureRepository = conduitClosureRepository;

            Description = "A route node in a route network.";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");
            Field<RouteNodeKindEnumType>("NodeKind", "Kind of node");
            Field<RouteNodeFunctionKindEnumType>("NodeFunctionKind", "Function that the node do/provides");
            Field(x => x.Name, type: typeof(IdGraphType)).Description("Name of node managed by the utility");

            Field<GeometryType>(
               "geometry",
               resolve: context =>
               {
                   return new Geometry(context.Source.Geometry.GeoJsonType, context.Source.Geometry.GeoJsonCoordinates);
               });

            Field<LocationInfoType>(
               "locationInfo",
               resolve: context =>
               {
                   return context.Source.LocationInfo;
               });


            Field<ListGraphType<ConduitRelationType>>(
              "relatedConduits",
              arguments: new QueryArguments(
                  new QueryArgument<BooleanGraphType> { Name = "includeMultiConduits"},
                  new QueryArgument<BooleanGraphType> { Name = "includeSingleConduits"},
                  new QueryArgument<BooleanGraphType> { Name = "includeInnerConduits"},
                  new QueryArgument<StringGraphType> { Name = "conduitSegmentId" }
                  ),
              resolve: context =>
              {
                  var includeMultiConduits = context.GetArgument<Boolean>("includeMultiConduits", true);
                  var includeSingleConduits = context.GetArgument<Boolean>("includeSingleConduits", true);
                  var includeInnerConduits = context.GetArgument<Boolean>("includeInnerConduits", true);

                  var conduitSegmentIdParam = context.GetArgument<string>("conduitSegmentId");

                  var conduitSegmentId = Guid.Empty;

                  if (conduitSegmentIdParam != null)
                  {
                      if (!Guid.TryParse(conduitSegmentIdParam, out conduitSegmentId))
                      {
                          context.Errors.Add(new ExecutionError("Wrong value for guid"));
                          return null;
                      }
                  }

                  List<ConduitRelation> result = new List<ConduitRelation>();

                  var conduitSegmentRels = conduitNetworkEqueryService.GetConduitSegmentsRelatedToPointOfInterest(context.Source.Id, conduitSegmentIdParam);

                  foreach (var conduitSegmentRel in conduitSegmentRels)
                  {
                      ConduitRelation rel = new ConduitRelation()
                      {
                          RelationType = conduitSegmentRel.Type,
                          Conduit = conduitSegmentRel.Segment.Conduit,
                          ConduitSegment = conduitSegmentRel.Segment
                      };

                      if (includeMultiConduits && conduitSegmentRel.Segment.Conduit.Kind == ConduitKindEnum.MultiConduit)
                          result.Add(rel);

                      if (includeInnerConduits && conduitSegmentRel.Segment.Conduit.Kind == ConduitKindEnum.InnerConduit)
                          result.Add(rel);

                      if (includeSingleConduits && conduitSegmentRel.Segment.Conduit.Kind == ConduitKindEnum.SingleConduit)
                          result.Add(rel);

                  }

                  return result;
              }
              );

            Field<ConduitClosureType>(
               "conduitClosure",
               resolve: context =>
               {
                   if (conduitClosureRepository.CheckIfConduitClosureAlreadyAddedToPointOfInterest(context.Source.Id))
                       return conduitClosureRepository.GetConduitClosureInfoByPointOfInterestId(context.Source.Id);
                   else
                       return null;
               });


            Field<RouteNodeGraphFunctions>("graph", resolve: context =>  context.Source);

        }
    }
}
