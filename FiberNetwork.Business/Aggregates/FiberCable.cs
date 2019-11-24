using FiberNetwork.Events;
using FiberNetwork.QueryService;
using Infrastructure.EventSourcing;
using RouteNetwork.QueryService;
using System;

namespace FiberNetwork.Business.Aggregates
{
    public class FiberCable : AggregateBase
    {
        private FiberCable()
        {
            // Register the event types that make up our aggregate , together with their respective handlers
            Register<FiberCablePlaced>(Apply);
            Register<FiberConnected>(Apply);
        }

        public FiberCable(Guid fiberCableId, Guid walkOfInterestId, int numberOfFiber, string name, IFiberNetworkQueryService fiberNetworkQueryService) : this()
        {
            // Id check
            if (fiberCableId == null || fiberCableId == Guid.Empty)
                throw new ArgumentException("Id cannot be null or empty");

            // Walk of interest id check
            if (walkOfInterestId == null || walkOfInterestId == Guid.Empty)
                throw new ArgumentException("WalkOfInterestId cannot be null or empty");

            // Check that not already exists
            if (fiberNetworkQueryService.CheckIfFiberCableIdExists(fiberCableId))
                throw new ArgumentException("A fiber cable with id: " + fiberCableId + " already exists");

            var fiberCablePlaceEvent = new FiberCablePlaced()
            {
                WalkOfInterestId = walkOfInterestId,
                FiberCableId = fiberCableId,
                NumberOfFibers = numberOfFiber,
            };

            RaiseEvent(fiberCablePlaceEvent);
        }

        internal void ConnectFiber(Guid pointOfInterestId, int fiberSequenceNumber /*, ConduitEndKindEnum endKind */, Guid junctionId, IRouteNetworkState routeNetworkQueryService, IFiberNetworkQueryService fiberNetworkQueryService)
        {
            // Point of interest id check
            if (pointOfInterestId == null || pointOfInterestId == Guid.Empty)
                throw new ArgumentException("PointOfInterestId cannot be null or empty");

            /*
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

            ILineSegment connectingSegment = null;

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

            */
        }


        // Apply multi conduit placement event
        private void Apply(FiberCablePlaced @event)
        {
            Id = @event.FiberCableId;
        }

        // Apply inner conduit addition event
        private void Apply(FiberConnected @event)
        {
        }
    }
}
