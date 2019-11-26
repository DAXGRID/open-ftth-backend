using Asset.Model;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel;
using Core.ReadModel.Network;
using FiberNetwork.Events.Model;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
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
        IRouteNetworkState routeNetworkQueryService;
        IConduitNetworkQueryService conduitNetworkEqueryService;
        IConduitClosureRepository conduitClosureRepository;

        public RouteNodeType(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IConduitClosureRepository conduitClosureRepository, IDataLoaderContextAccessor dataLoader, IHttpContextAccessor httpContextAccessor)
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
                  new QueryArgument<BooleanGraphType> { Name = "includeMultiConduits" },
                  new QueryArgument<BooleanGraphType> { Name = "includeSingleConduits" },
                  new QueryArgument<BooleanGraphType> { Name = "includeInnerConduits" },
                  new QueryArgument<StringGraphType> { Name = "conduitSegmentId", Description = "Will be deleted. Use conduit id instead" },
                   new QueryArgument<StringGraphType> { Name = "conduitId", Description = "Id of conduit of conduit segment" }
                  ),
              resolve: context =>
              {
                  var includeMultiConduits = context.GetArgument<Boolean>("includeMultiConduits", true);
                  var includeSingleConduits = context.GetArgument<Boolean>("includeSingleConduits", true);
                  var includeInnerConduits = context.GetArgument<Boolean>("includeInnerConduits", true);

                  var conduitSegmentIdParam = context.GetArgument<string>("conduitSegmentId");
                  var conduitIdParam = context.GetArgument<string>("conduitId");

                  var conduitSegmentId = Guid.Empty;

                  if (conduitSegmentIdParam != null)
                  {
                      if (!Guid.TryParse(conduitSegmentIdParam, out conduitSegmentId))
                      {
                          context.Errors.Add(new ExecutionError("Wrong value for guid"));
                          return null;
                      }
                  }

                  if (conduitIdParam != null)
                  {
                      if (!Guid.TryParse(conduitIdParam, out conduitSegmentId))
                      {
                          context.Errors.Add(new ExecutionError("Wrong value for guid"));
                          return null;
                      }
                  }

                  List<ConduitRelation> result = new List<ConduitRelation>();

                  var conduitSegmentRels = conduitNetworkEqueryService.GetConduitSegmentsRelatedToPointOfInterest(context.Source.Id, conduitSegmentId == Guid.Empty ? null : conduitSegmentId.ToString());



                  foreach (var conduitSegmentRel in conduitSegmentRels)
                  {
                      ConduitRelation rel = new ConduitRelation()
                      {
                          RelationType = Convert(conduitSegmentRel.RelationType),
                          Conduit = ((ConduitSegmentInfo)conduitSegmentRel.Segment).Conduit,
                          ConduitSegment = (ConduitSegmentInfo)conduitSegmentRel.Segment
                      };

                      // Check if segment is related to a conduit closure
                      {
                          if (conduitClosureRepository.CheckIfRouteNodeContainsConduitClosure(context.Source.Id))
                          {
                              var conduitClosureInfo = conduitClosureRepository.GetConduitClosureInfoByRouteNodeId(context.Source.Id);

                              // If conduit closure contains a port or terminal related to the conduit segment, then it's related to the conduit closure
                              if (
                                  conduitClosureInfo.Sides.Exists(s => s.Ports.Exists(p => p.MultiConduitSegmentId == conduitSegmentRel.Segment.Id))
                                  ||
                                  conduitClosureInfo.Sides.Exists(s => s.Ports.Exists(p => p.Terminals.Exists(t => t.LineSegmentId == conduitSegmentRel.Segment.Id)))
                              )
                                  rel.CanBeAttachedToConduitClosure = false;
                              else
                                  rel.CanBeAttachedToConduitClosure = true;
                          }

                      }

                      // Check if segment is cut at node
                      if (conduitSegmentRel.Segment.Line.Segments.Exists(s => s.FromRouteNodeId == context.Source.Id || s.ToRouteNodeId == context.Source.Id))
                          rel.CanBeCutAtNode = false;
                      else
                          rel.CanBeCutAtNode = true;


                      if (includeMultiConduits && conduitSegmentRel.Segment.Line.LineKind == LineKindEnum.MultiConduit)
                          result.Add(rel);

                      if (includeInnerConduits && conduitSegmentRel.Segment.Line.LineKind == LineKindEnum.InnerConduit)
                          result.Add(rel);

                      if (includeSingleConduits && conduitSegmentRel.Segment.Line.LineKind == LineKindEnum.SingleConduit)
                          result.Add(rel);

                  }

                  return result;
              }
              );

            Field<ConduitClosureType>(
               "conduitClosure",
               resolve: context =>
               {
                   if (conduitClosureRepository.CheckIfRouteNodeContainsConduitClosure(context.Source.Id))
                       return conduitClosureRepository.GetConduitClosureInfoByRouteNodeId(context.Source.Id);
                   else
                       return null;
               });


            Field<RouteNodeGraphFunctions>("graph", resolve: context => context.Source);

            Field<ListGraphType<LineSegmentInterface>>(
              "relatedSegments",
              arguments: new QueryArguments(
                   new QueryArgument<StringGraphType> { Name = "lineId", Description = "Id of specific line og line segment to fetch" }
              ),
              resolve: context =>
              {
                  httpContextAccessor.HttpContext.Items.Add("routeNodeId", context.Source.Id);

                  List<ILineSegment> result = new List<ILineSegment>();

                  var conduitSegmentsWithRel = conduitNetworkEqueryService.GetConduitSegmentsRelatedToPointOfInterest(context.Source.Id);

                  HashSet<Guid> segmentExistsLookup = new HashSet<Guid>();
                  foreach (var conduitSegmentWithRel in conduitSegmentsWithRel)
                  {
                      segmentExistsLookup.Add(conduitSegmentWithRel.Segment.Id);
                  }

                  foreach (var conduitSegmentWithRel in conduitSegmentsWithRel)
                  {
                      // First we filter by kind. Only multi conduit, single conduit and fiber cable could possible be roots of a node
                      if (conduitSegmentWithRel.Segment.Line.LineKind == LineKindEnum.MultiConduit
                      || conduitSegmentWithRel.Segment.Line.LineKind == LineKindEnum.SingleConduit
                      || conduitSegmentWithRel.Segment.Line.LineKind == LineKindEnum.FiberCable
                      )
                      {
                          // Now check if the segment has a parent that is also related to the node. 
                          // If so, the segment is contained within another segment, and should not be returned as a root element to the node
                          bool isContained = false;

                          if (conduitSegmentWithRel.Segment.Parents != null)
                          {
                              foreach (var parent in conduitSegmentWithRel.Segment.Parents)
                              {
                                  if (segmentExistsLookup.Contains(parent.Id))
                                      isContained = true;
                              }
                          }

                          if (!isContained)
                            result.Add(conduitSegmentWithRel.Segment);
                      }
                      
                  }

                  return result;

              });
        }

        private ConduitRelationTypeEnum Convert(LineSegmentRelationTypeEnum input)
        {
            if (input == LineSegmentRelationTypeEnum.Incomming)
                return ConduitRelationTypeEnum.Incomming;
            else if (input == LineSegmentRelationTypeEnum.Outgoing)
                return ConduitRelationTypeEnum.Outgoing;
            else if (input == LineSegmentRelationTypeEnum.PassBy)
                return ConduitRelationTypeEnum.PassBy;
            else if (input == LineSegmentRelationTypeEnum.PassThrough)
                return ConduitRelationTypeEnum.PassThrough;
            else
                return ConduitRelationTypeEnum.Incomming;
        }
    }
}
