using Asset.Model;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel;
using Core.ReadModel.Network;
using FiberNetwork.Events.Model;
using FiberNetwork.QueryService;
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

        public RouteNodeType(IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IFiberNetworkQueryService fiberNetworkQueryService, IConduitClosureRepository conduitClosureRepository, IDataLoaderContextAccessor dataLoader, IHttpContextAccessor httpContextAccessor)
        {
            this.routeNetworkQueryService = routeNetworkQueryService;
            this.conduitNetworkEqueryService = conduitNetworkEqueryService;
            this.conduitClosureRepository = conduitClosureRepository;

            Description = "A route node in a route network.";

            // Interface fields

            Interface<NodeInterface>();

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");

            Field(x => x.Name, type: typeof(IdGraphType)).Description("Name of node managed by the utility");


            // Additional fields

            Field<RouteNodeKindEnumType>("NodeKind", "Kind of node");
            Field<RouteNodeFunctionKindEnumType>("NodeFunctionKind", "Function that the node do/provides");
         
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

            Field<ListGraphType<SegmentInterface>>(
              "relatedSegments",
              arguments: new QueryArguments(
                   new QueryArgument<StringGraphType> { Name = "lineId", Description = "Id of specific line og line segment to fetch" }
              ),
              resolve: context =>
              {
                  httpContextAccessor.HttpContext.Items.Add("routeNodeId", context.Source.Id);

                  List<ISegment> result = new List<ISegment>();

                  var allSegments = new List<SegmentWithRouteNodeRelationInfo>();

                  // Add multi conduits and single conduits segments
                  allSegments.AddRange(conduitNetworkEqueryService.GetConduitSegmentsRelatedToPointOfInterest(context.Source.Id).Where(c => c.Segment.Line.LineKind == LineKindEnum.MultiConduit || c.Segment.Line.LineKind == LineKindEnum.SingleConduit));
                  // Add fiber cable segments
                  allSegments.AddRange(fiberNetworkQueryService.GetLineSegmentsRelatedToPointOfInterest(context.Source.Id).Where(c => c.Segment.Line.LineKind == LineKindEnum.FiberCable));

                  // Create fast lookup table to be used for parent check

                  HashSet<Guid> segmentExistsLookup = new HashSet<Guid>();
                  foreach (var segmentWithRel in allSegments)
                      segmentExistsLookup.Add(segmentWithRel.Segment.Id);

                  // Iterate through all segments and decided if to return or not depending on if they are roots of the node or not
                  foreach (var segmentWithRel in allSegments)
                  {
                      // Now check if the segment has a parent that is also related to the node. 
                      // If so, the segment is contained within another segment, and should not be returned as a root element to the node
                      bool isContained = false;

                      if (segmentWithRel.Segment.Parents != null)
                      {
                          foreach (var parent in segmentWithRel.Segment.Parents)
                          {
                              if (segmentExistsLookup.Contains(parent.Id))
                                  isContained = true;
                          }
                      }

                      if (!isContained)
                          result.Add(segmentWithRel.Segment);
                  }

                  return result;

              });
        }

        private ConduitRelationTypeEnum Convert(SegmentRelationTypeEnum input)
        {
            if (input == SegmentRelationTypeEnum.Incomming)
                return ConduitRelationTypeEnum.Incomming;
            else if (input == SegmentRelationTypeEnum.Outgoing)
                return ConduitRelationTypeEnum.Outgoing;
            else if (input == SegmentRelationTypeEnum.PassBy)
                return ConduitRelationTypeEnum.PassBy;
            else if (input == SegmentRelationTypeEnum.PassThrough)
                return ConduitRelationTypeEnum.PassThrough;
            else
                return ConduitRelationTypeEnum.Incomming;
        }
    }
}
