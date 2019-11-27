using Asset.Model;
using ConduitNetwork.Business.DemoDataBuilder;
using ConduitNetwork.Business.Specifications;
using ConduitNetwork.Events;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using Core.ReadModel.Network;
using Infrastructure.EventSourcing;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConduitNetwork.Business.Aggregates
{
    public class MultiConduit : AggregateBase
    {
        private ConduitInfo _outerConduitInfo;

        private AssetInfo _assetInfo;

        private MultiConduit()
        {
            // Register the event types that make up our aggregate , together with their respective handlers
            Register<MultiConduitPlaced>(Apply);
            Register<MultiConduitInnerConduitAdded>(Apply);
            Register<MultiConduitOuterConduitCut>(Apply);
            Register<MultiConduitInnerConduitCut>(Apply);
            Register<MultiConduitInnerConduitConnected>(Apply);
        }

        public MultiConduit(Guid conduitId, Guid walkOfInterestId, Guid conduitSpecificationId, string name, ConduitColorEnum markingColor, string markingText, IConduitNetworkQueryService conduitNetworkQueryService, IConduitSpecificationRepository conduitSpecificationRepository, string demoDataSpec = null) : this()
        {
            // Conduit Id check
            if (conduitId == null || conduitId == Guid.Empty)
                throw new ArgumentException("Id cannot be null or empty");

            // Walk of interest id check
            if (walkOfInterestId == null || walkOfInterestId == Guid.Empty)
                throw new ArgumentException("WalkOfInterestId cannot be null or empty");

            // Check that not already exists
            if (conduitNetworkQueryService.CheckIfMultiConduitIdExists(conduitId))
                throw new ArgumentException("A multi conduit id: " + conduitId + " already exists");

            // Create the multi conduit itself
            if (demoDataSpec != null && demoDataSpec != "")
            {
                var multiConduitPlaced = ConduitEventBuilder.CreateMultiConduitPlacedEvent(conduitId, walkOfInterestId, demoDataSpec);
                RaiseEvent(multiConduitPlaced);

                // Create all the inner conduits (if the multi conduit has such - the demo data builder will know)
                if (!demoDataSpec.StartsWith("FLEX"))
                {
                    var innerConduitAddedEvents = ConduitEventBuilder.CreateInnerConduitAddedEvents(multiConduitPlaced, demoDataSpec);

                    foreach (var innerConduitAddedEvent in innerConduitAddedEvents)
                        RaiseEvent(innerConduitAddedEvent);
                }
            }
            else
            {
                var conduitSpec = conduitSpecificationRepository.GetConduitSpecification(conduitSpecificationId);

                var assetInfo = new AssetInfo();
                assetInfo.Model = conduitSpec.ProductModels[0];
                assetInfo.Manufacturer = conduitSpec.ProductModels[0].Manufacturer;

                var conduitInfo = new ConduitInfo()
                {
                    Id = conduitId,
                    Name = name,
                    Shape = conduitSpec.Shape,
                    Color = conduitSpec.Color,
                    ColorMarking = markingColor,
                    TextMarking = markingText,
                    OuterDiameter = conduitSpec.OuterDiameter,
                    InnerDiameter = conduitSpec.InnerDiameter
                };

                var multiConduitEvent = new MultiConduitPlaced()
                {
                    WalkOfInterestId = walkOfInterestId,
                    MultiConduitId = conduitId,
                    ConduitInfo = conduitInfo,
                    AssetInfo = assetInfo
                };

                RaiseEvent(multiConduitEvent);


                // Create all the inner conduit
                foreach (var innerConduitSpec in conduitSpec.ChildSpecifications)
                {
                    var innerConduitInfo = new ConduitInfo()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Subrør " + innerConduitSpec.SequenceNumber,
                        Color = innerConduitSpec.Color,
                        InnerDiameter = innerConduitSpec.InnerDiameter,
                        OuterDiameter = innerConduitSpec.OuterDiameter,
                        Shape = innerConduitSpec.Shape,
                        ColorMarking = ConduitColorEnum.None
                    };

                    var innerConduitAddedEvent = new MultiConduitInnerConduitAdded()
                    {
                        MultiConduitId = conduitId,
                        MultiConduitIndex = innerConduitSpec.SequenceNumber,
                        ConduitInfo = innerConduitInfo
                    };

                    RaiseEvent(innerConduitAddedEvent);
                }

            }

         
        }

        internal void CutOuterConduit(Guid pointOfInterestId, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService)
        {
            // Point of interest id check
            if (pointOfInterestId == null || pointOfInterestId == Guid.Empty)
                throw new ArgumentException("PointOfInterestId cannot be null or empty");

            var multiConduitInfo = conduitNetworkQueryService.GetMultiConduitInfo(Id);

            if (multiConduitInfo.Segments.Exists(s => s.FromRouteNodeId == pointOfInterestId || s.ToRouteNodeId == pointOfInterestId))
                throw new ArgumentException("Multi conduit: " + Id + " is already cut at: " + pointOfInterestId);

            // Check that conduit is cut at a node part of conduit walk of interest
            var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(multiConduitInfo.WalkOfInterestId);

            if (!walkOfInterest.AllNodeIds.Contains(pointOfInterestId))
                throw new ArgumentException("The point of interest: " + pointOfInterestId + " was not found in walk of interest:" + multiConduitInfo.WalkOfInterestId + " of multi conduit: " + Id);

            // Check that conduit is not cut at ends 
            if (walkOfInterest.StartNodeId == pointOfInterestId || walkOfInterest.EndNodeId == pointOfInterestId)
                throw new ArgumentException("The point of interest: " + pointOfInterestId + " is one of the ends in walk of interest:" + multiConduitInfo.WalkOfInterestId + " of multi conduit: " + Id + " This is not allowed. You cannot cut a conduit at its ends.");


            RaiseEvent(new MultiConduitOuterConduitCut
            {
                MultiConduitId = Id,
                PointOfInterestId = pointOfInterestId
            });
        }

       

        public void CutInnerConduit(int sequenceNumber, Guid pointOfInterestId, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService)
        {
            var multiConduitInfo = conduitNetworkQueryService.GetMultiConduitInfo(Id);

            // Point of interest id check
            if (pointOfInterestId == null || pointOfInterestId == Guid.Empty)
                throw new ArgumentException("PointOfInterestId cannot be null or empty");

            // Inner conduit number check
            if (!multiConduitInfo.Children.OfType<ConduitInfo>().Any(i => i.SequenceNumber == sequenceNumber))
                throw new ArgumentException("Cannot find inner conduit number: " + sequenceNumber + " in multi conduit: " + Id);


            var singleConduitInfo = conduitNetworkQueryService.GetSingleConduitInfo(multiConduitInfo.Children.OfType<ConduitInfo>().Single(i => i.SequenceNumber == sequenceNumber).Id);

            // Multi conduit cut check
            if (!multiConduitInfo.Segments.Exists(s => s.FromRouteNodeId == pointOfInterestId || s.ToRouteNodeId == pointOfInterestId))
                throw new ArgumentException("Multi conduit: " + Id + " is not cut at: " + pointOfInterestId + " You cannot cut a inner conduit before the outer conduit is cut.");

            // Inner conduit cut check
            if (singleConduitInfo.Segments.Exists(s => s.FromRouteNodeId == pointOfInterestId || s.ToRouteNodeId == pointOfInterestId))
                throw new ArgumentException("Inner conduit number: " + sequenceNumber + " in multi conduit: " + Id + " is already cut at: " + pointOfInterestId);

            // Check that conduit is cut at a node part of conduit walk of interest
            var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(multiConduitInfo.WalkOfInterestId);

            if (!walkOfInterest.AllNodeIds.Contains(pointOfInterestId))
                throw new ArgumentException("The point of interest: " + pointOfInterestId + " was not found in walk of interest:" + multiConduitInfo.WalkOfInterestId + " of multi conduit: " + Id);

            // Check that conduit is not cut at ends 
            if (walkOfInterest.StartNodeId == pointOfInterestId || walkOfInterest.EndNodeId == pointOfInterestId)
                throw new ArgumentException("The point of interest: " + pointOfInterestId + " is one of the ends in walk of interest:" + multiConduitInfo.WalkOfInterestId + " of multi conduit: " + Id + " This is not allowed. You cannot cut a conduit at its ends.");


            RaiseEvent(new MultiConduitInnerConduitCut
            {
                MultiConduitId = Id,
                InnerConduitSequenceNumber = sequenceNumber,
                PointOfInterestId = pointOfInterestId
            });
        }

        internal int AddInnerConduit(ConduitColorEnum color, int outerDiameter, int innerDiameter, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService)
        {
            var multiConduitInfo = conduitNetworkQueryService.GetMultiConduitInfo(Id);

            int seqNo = 1;

            if (multiConduitInfo.Children != null && multiConduitInfo.Children.Count > 0)
            {
                var lastInnerConduitSeqNo = multiConduitInfo.Children.OfType<ConduitInfo>().Max(c => c.SequenceNumber);
                seqNo = lastInnerConduitSeqNo + 1;
            }

            var innerConduitInfo = new ConduitInfo()
            {
                Id = Guid.NewGuid(),
                Name = "Subrør " + seqNo,
                Color = color,
                Kind = ConduitKindEnum.InnerConduit,
                InnerDiameter = innerDiameter,
                OuterDiameter = outerDiameter,
                Shape = ConduitShapeKindEnum.Round,
                ColorMarking = ConduitColorEnum.None
            };

            var innerConduitAddedEvent = new MultiConduitInnerConduitAdded()
            {
                MultiConduitId = Id,
                MultiConduitIndex = seqNo,
                ConduitInfo = innerConduitInfo
            };

            RaiseEvent(innerConduitAddedEvent);

            return seqNo;
        }

        internal void ConnectInnerConduit(Guid pointOfInterestId, int sequenceNumber, ConduitEndKindEnum endKind, Guid junctionId, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService)
        {
            // Point of interest id check
            if (pointOfInterestId == null || pointOfInterestId == Guid.Empty)
                throw new ArgumentException("PointOfInterestId cannot be null or empty");

            // endKind check
            if (endKind == 0)
                throw new ArgumentException("End kind must be specified. Otherwise the system don't know with end of the segment to connect, if a conduit has been cut into two pieces in a node.");


            var multiConduitInfo = conduitNetworkQueryService.GetMultiConduitInfo(Id);

            // Inner conduit number check
            if (!multiConduitInfo.Children.OfType<ConduitInfo>().Any(i => i.SequenceNumber == sequenceNumber))
                throw new ArgumentException("Cannot find inner conduit number: " + sequenceNumber + " in multi conduit: " + Id);

            var singleConduitInfo = conduitNetworkQueryService.GetSingleConduitInfo(multiConduitInfo.Children.OfType<ConduitInfo>().Single(i => i.SequenceNumber == sequenceNumber).Id);

            if (!singleConduitInfo.Segments.Exists(s => s.FromRouteNodeId == pointOfInterestId || s.ToRouteNodeId == pointOfInterestId))
                throw new ArgumentException("Inner conduit number: " + sequenceNumber + " in multi conduit: " + Id + " is not cut at: " + pointOfInterestId);

            // Check that conduit is connected at a node part of conduit walk of interest
            var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(multiConduitInfo.WalkOfInterestId);

            if (!walkOfInterest.AllNodeIds.Contains(pointOfInterestId))
                throw new ArgumentException("The point of interest: " + pointOfInterestId + " was not found in walk of interest:" + multiConduitInfo.WalkOfInterestId + " of multi conduit: " + Id);

            // Check incomming
            if (endKind == ConduitEndKindEnum.Incomming && !multiConduitInfo.Segments.Exists(s => s.ToRouteNodeId == pointOfInterestId))
                throw new ArgumentException("No segments are incomming to point of interest: " + pointOfInterestId + " in multi conduit: " + Id);

            // Check outgoing
            if (endKind == ConduitEndKindEnum.Outgoing && !multiConduitInfo.Segments.Exists(s => s.FromRouteNodeId == pointOfInterestId))
                throw new ArgumentException("No segments are outgoing from point of interest: " + pointOfInterestId + " in multi conduit: " + Id);

            ISegment connectingSegment = null;

            // Check incomming inner conduit
            if (endKind == ConduitEndKindEnum.Incomming)
            {
                if (!singleConduitInfo.Segments.Exists(s => s.ToRouteNodeId == pointOfInterestId))
                    throw new ArgumentException("No inner conduit segments are incomming to point of interest: " + pointOfInterestId + " in single conduit: " + Id);

                connectingSegment = singleConduitInfo.Segments.Find(s => s.ToRouteNodeId == pointOfInterestId);

                // Check if already connect to a junction
                if (connectingSegment.ToNodeId != Guid.Empty)
                    throw new ArgumentException("the incomming  inner conduit segment: " + connectingSegment.Id + " is already connected to a junction: " + connectingSegment.ToNodeId);
            }
            // Check outgoing inner conduit
            else
            {
                if (!singleConduitInfo.Segments.Exists(s => s.FromRouteNodeId == pointOfInterestId))
                    throw new ArgumentException("No  inner conduit segments are outgoing from point of interest: " + pointOfInterestId + " in single conduit: " + Id);

                connectingSegment = singleConduitInfo.Segments.Find(s => s.FromRouteNodeId == pointOfInterestId);

                // Check if already connect to a junction
                if (connectingSegment.FromNodeId != Guid.Empty)
                    throw new ArgumentException("the outgoing  inner conduit segment: " + connectingSegment.Id + " is already connected to a junction: " + connectingSegment.FromNodeId);
            }


            RaiseEvent(new MultiConduitInnerConduitConnected
            {
                MultiConduitId = Id,
                PointOfInterestId = pointOfInterestId,
                InnerConduitSequenceNumber = sequenceNumber,
                ConnectedEndKind = endKind,
                ConnectedJunctionId = junctionId
            });
        }

        public void ContinueInnerConduitIntoAnotherMultiConduit(Guid pointOfInterestId, Guid fromConduitSegmentId, Guid toConduitSegmentId, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService)
        {
            var multiConduitInfo = conduitNetworkQueryService.GetMultiConduitInfo(Id);

            var fromSegment = multiConduitInfo.Segments.Find(s => s.Id == fromConduitSegmentId);

            // Junction id
            Guid junctionId = Guid.NewGuid();

            // Find from direction
            ConduitEndKindEnum fromEndKind = ConduitEndKindEnum.Incomming;

            if (fromSegment.FromRouteNodeId == pointOfInterestId)
                fromEndKind = ConduitEndKindEnum.Outgoing;

            RaiseEvent(new MultiConduitInnerConduitConnected
            {
                MultiConduitId = Id,
                PointOfInterestId = pointOfInterestId,
                InnerConduitSequenceNumber = fromSegment.Line.SequenceNumber,
                ConnectedEndKind = fromEndKind,
                ConnectedJunctionId = junctionId
            });
        }

        public ConduitInfo OuterConduit { get { return _outerConduitInfo; } }

        public AssetInfo AssetInfo { get { return _assetInfo; } }


        // Apply multi conduit placement event
        private void Apply(MultiConduitPlaced @event)
        {
            Id = @event.ConduitInfo.Id;
            _outerConduitInfo = @event.ConduitInfo;
            _assetInfo = @event.AssetInfo;
        }

        // Apply inner conduit addition event
        private void Apply(MultiConduitInnerConduitAdded @event)
        {
        }

        // Apply inner conduit cut event
        private void Apply(MultiConduitInnerConduitCut @event)
        {
        }

        // Apply outer conduit cut event
        private void Apply(MultiConduitOuterConduitCut @event)
        {
        }

        // Apply inner conduit connected event
        private void Apply(MultiConduitInnerConduitConnected @event)
        {
        }
    }

    public class InnerConduitCut
    {
        public Guid PointOfInterestId { get; set; }
        public int InnerConduitSequenceNumber { get; set; }
    }
}
