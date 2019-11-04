using Asset.Model;
using ConduitNetwork.Business.DemoDataBuilder;
using ConduitNetwork.Business.Specifications;
using ConduitNetwork.Events;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using Infrastructure.EventSourcing;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConduitNetwork.Business.Aggregates
{
    public class SingleConduit : AggregateBase
    {
        private ConduitInfo _outerConduitInfo;

        private AssetInfo _assetInfo;

        private SingleConduit()
        {
            // Register the event types that make up our aggregate , together with their respective handlers
            Register<SingleConduitPlaced>(Apply);
            Register<SingleConduitCut>(Apply);
            Register<SingleConduitConnected>(Apply);
        }

        public SingleConduit(Guid conduitId, Guid walkOfInterestId, Guid conduitSpecificationId, string name, ConduitColorEnum markingColor, string markingText, IConduitNetworkQueryService conduitNetworkQueryService, IConduitSpecificationRepository conduitSpecificationRepository, string demoDataSpec = null) : this()
        {
            //////////////////////////////////////////////////////////////////////////////////////
            // NOTICE:
            // This constructor is currently a hack that just uses the demo data builder.
            // Must be refactored to use a conduit catalog system.

            // Conduit Id check
            if (conduitId == null || conduitId == Guid.Empty)
                throw new ArgumentException("Id cannot be null or empty");

            // Walk of interest id check
            if (walkOfInterestId == null || walkOfInterestId == Guid.Empty)
                throw new ArgumentException("WalkOfInterestId cannot be null or empty");

            // Check that not already exists
            if (conduitNetworkQueryService.CheckIfSingleConduitIdExists(conduitId))
                throw new ArgumentException("A singe conduit id: " + conduitId + " already exists");

            // Create the conduit
            if (demoDataSpec != null && demoDataSpec != "")
            {
                var singleConduitPlaced = ConduitEventBuilder.CreateSingleConduitPlacedEvent(conduitId, walkOfInterestId, demoDataSpec);
                RaiseEvent(singleConduitPlaced);
            }
            else
            {
                var conduitSpec = conduitSpecificationRepository.GetConduitSpecification(conduitSpecificationId);

                var assetInfo = new AssetInfo();
                if (conduitSpec.ProductModels != null && conduitSpec.ProductModels.Count > 0)
                {
                    assetInfo.Model = conduitSpec.ProductModels[0];

                    if (conduitSpec.ProductModels[0].Manufacturer != null)
                        assetInfo.Manufacturer = conduitSpec.ProductModels[0].Manufacturer;
                }

                var conduitInfo = new ConduitInfo()
                {
                    Id = conduitId,
                    Name = name,
                    Shape = conduitSpec.Shape,
                    Color = conduitSpec.Color,
                    ColorMarking = markingColor,
                    OuterDiameter = conduitSpec.OuterDiameter,
                    InnerDiameter = conduitSpec.InnerDiameter
                };

                var singleConduitPlaccedEvent = new SingleConduitPlaced()
                {
                    WalkOfInterestId = walkOfInterestId,
                    SingleConduitId = conduitId,
                    ConduitInfo = conduitInfo,
                    AssetInfo = assetInfo
                };

                RaiseEvent(singleConduitPlaccedEvent);
            }
        }

        internal void Cut(Guid pointOfInterestId, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService)
        {
            // Point of interest id check
            if (pointOfInterestId == null || pointOfInterestId == Guid.Empty)
                throw new ArgumentException("PointOfInterestId cannot be null or empty");

            // Check if single conduit is already cut
            var singleConduitInfo = conduitNetworkQueryService.GetSingleConduitInfo(Id);

            if (singleConduitInfo.Segments.Exists(s => s.FromNodeId == pointOfInterestId || s.ToNodeId == pointOfInterestId))
                throw new ArgumentException("Single conduit: " + Id + " is already cut at: " + pointOfInterestId);

            // Check that conduit is cut at a node part of conduit walk of interest
            var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(singleConduitInfo.GetRootConduit().WalkOfInterestId);

            if (!walkOfInterest.AllNodeIds.Contains(pointOfInterestId))
                throw new ArgumentException("The point of interest: " + pointOfInterestId + " was not found in walk of interest:" + singleConduitInfo.WalkOfInterestId + " of single conduit: " + Id);

            // Check that conduit is not cut at ends 
            if (walkOfInterest.StartNodeId == pointOfInterestId || walkOfInterest.EndNodeId == pointOfInterestId)
                throw new ArgumentException("The point of interest: " + pointOfInterestId + " is one of the ends in walk of interest:" + singleConduitInfo.WalkOfInterestId + " of single conduit: " + Id + " This is not allowed. You cannot cut a conduit at its ends.");

            RaiseEvent(new SingleConduitCut
            {
                SingleConduitId = Id,
                PointOfInterestId = pointOfInterestId
            });
        }

        internal void Connect(Guid pointOfInterestId, ConduitEndKindEnum endKind, Guid junctionId, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService)
        {
            var singleConduitInfo = conduitNetworkQueryService.GetSingleConduitInfo(Id);

            // endKind check
            if (endKind == 0)
                throw new ArgumentException("End kind must be specified. Otherwise the system don't know with end of the segment to connect, if a conduit has been cut into two pieces in a node.");

            if (!singleConduitInfo.Segments.Exists(s => s.FromNodeId == pointOfInterestId || s.ToNodeId == pointOfInterestId))
                throw new ArgumentException("The single conduit: " + Id + " is not cut at: " + pointOfInterestId);

            // Check that conduit is connected at a node part of conduit walk of interest
            var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(singleConduitInfo.GetRootConduit().WalkOfInterestId);

            if (!walkOfInterest.AllNodeIds.Contains(pointOfInterestId))
                throw new ArgumentException("The point of interest: " + pointOfInterestId + " was not found in walk of interest:" + singleConduitInfo.WalkOfInterestId + " of single conduit: " + Id);

            ConduitSegmentInfo connectingSegment = null;

            // Check incomming
            if (endKind == ConduitEndKindEnum.Incomming)
            {
                if (!singleConduitInfo.Segments.Exists(s => s.ToNodeId == pointOfInterestId))
                    throw new ArgumentException("No segments are incomming to point of interest: " + pointOfInterestId + " in single conduit: " + Id);

                connectingSegment = singleConduitInfo.Segments.Find(s => s.ToNodeId == pointOfInterestId);

                // Check if already connect to a junction
                if (connectingSegment.ToJunctionId != Guid.Empty)
                    throw new ArgumentException("the incomming segment: " + connectingSegment.Id + " is already connected to a junction: " + connectingSegment.ToJunctionId);
            }
            // Check outgoing
            else
            {
                if (!singleConduitInfo.Segments.Exists(s => s.FromNodeId == pointOfInterestId))
                    throw new ArgumentException("No segments are outgoing from point of interest: " + pointOfInterestId + " in single conduit: " + Id);

                connectingSegment = singleConduitInfo.Segments.Find(s => s.FromNodeId == pointOfInterestId);

                // Check if already connect to a junction
                if (connectingSegment.FromJunctionId != Guid.Empty)
                    throw new ArgumentException("the outgoing segment: " + connectingSegment.Id + " is already connected to a junction: " + connectingSegment.FromJunctionId);
            }



            RaiseEvent(new SingleConduitConnected
            {
                SingleConduitId = Id,
                PointOfInterestId = pointOfInterestId,
                ConnectedEndKind = endKind,
                ConnectedJunctionId = junctionId
            });
        }

        public ConduitInfo OuterConduit { get { return _outerConduitInfo; } }

        public AssetInfo AssetInfo { get { return _assetInfo; } }


        // Apply conduit placement event
        private void Apply(SingleConduitPlaced @event)
        {
            Id = @event.ConduitInfo.Id;
            _outerConduitInfo = @event.ConduitInfo;
            _assetInfo = @event.AssetInfo;
        }
     

        // Apply inner conduit cut event
        private void Apply(SingleConduitCut @event)
        {
        }

        // Apply inner conduit connected event
        private void Apply(SingleConduitConnected @event)
        {
        }
    }
}
