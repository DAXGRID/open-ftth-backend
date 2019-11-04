using ConduitNetwork.Events;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel;
using Infrastructure.EventSourcing;
using RouteNetwork.QueryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConduitNetwork.Business.Aggregates
{
    public class ConduitClosure : AggregateBase
    {
        private Guid _pointOfInterestId;

        private bool _removed = false;

        public ConduitClosure()
        {
            Register<ConduitClosurePlaced>(Apply);
            Register<ConduitClosureRemoved>(Apply);
            Register<ConduitClosurePassingByConduitAttached>(Apply);
            Register<ConduitClosureConduitEndAttached>(Apply);
        }

        public ConduitClosure(Guid conduitClosureId, Guid pointOfInterestId, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IConduitClosureRepository conduitClosureRepository) : this()
        {
            // Id check
            if (conduitClosureId == null || conduitClosureId == Guid.Empty)
                throw new ArgumentException("ConduitClosureId cannot be null or empty");

            // Point of interest id check
            if (pointOfInterestId == null || pointOfInterestId == Guid.Empty)
                throw new ArgumentException("PointOfInterestId cannot be null or empty");

            // Check that point of interest (node) exists
            if (!routeNetworkQueryService.CheckIfRouteNodeIdExists(pointOfInterestId))
                throw new ArgumentException("PointOfInterestId: " + pointOfInterestId + " not found in the route network.");

            // Check that conduit closure do not already exists
            if (conduitClosureRepository.CheckIfConduitClosureAlreadyExists(conduitClosureId))
                throw new ArgumentException("A conduit closure with id: " + conduitClosureId + " already exists.");

            // Check that a conduit closure is not already added to point of interest. This is not allowed, each conduit closure must be placed in its own node.
            if (conduitClosureRepository.CheckIfConduitClosureAlreadyAddedToPointOfInterest(pointOfInterestId))
                throw new ArgumentException("A conduit closure: " + conduitClosureRepository.GetConduitClosureInfoByPointOfInterestId(pointOfInterestId).Id + " is already placed in the specified point of interest (route node): " + pointOfInterestId + " Only one conduit closure is allowed per point of interest (route node).");


            var conduitClosurePlaced = new ConduitClosurePlaced()
            {
                ConduitClosureId = conduitClosureId,
                PointOfInterestId = pointOfInterestId
            };

            RaiseEvent(conduitClosurePlaced);
        }

        internal void Remove()
        {
            if (_removed)
                throw new ArgumentException("The conduit closure: " + Id + " is allready removed.");

            var conduitClosureRemoved = new ConduitClosureRemoved()
            {
                ConduitClosureId = Id
            };

            RaiseEvent(conduitClosureRemoved);
        }

        internal void AttachPassByConduitToClosure(Guid conduitId, ConduitClosureSideEnum incommingSide, ConduitClosureSideEnum outgoingSide, int incommingPortPosition, int outgoingPortPosition, int incommingTerminalPosition, int outgoingTerminalPosition,IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IConduitClosureRepository conduitClosureRepository)
        {

            // Check if multi conduit is passing by closure
            var conduit = conduitNetworkQueryService.GetMultiConduitInfo(conduitId);
            var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(conduit.WalkOfInterestId);

            if (!walkOfInterest.AllNodeIds.Contains(_pointOfInterestId))
                throw new ArgumentException("Conduit: " + conduitId + " is not related to the point of interest: " + _pointOfInterestId + " where conduit closure: " + Id + " is placed at all.");

            if (walkOfInterest.StartNodeId == _pointOfInterestId || walkOfInterest.EndNodeId == _pointOfInterestId)
                throw new ArgumentException("Conduit: " + conduitId + " is ending in point of interest: " + _pointOfInterestId + " - but not pasing through it. Please use AttachConduitEndToClosure instead. The AttachPassByConduitToClosure can only be used on conduits that are passing through the point of interest (route node) where the conduit closure is placed.");

            if (incommingSide == outgoingSide && incommingPortPosition == outgoingPortPosition)
                throw new ArgumentException("A conduit is not allowed to enter and exit the same port on the same side.");


            var conduitClosureInfo = conduitClosureRepository.GetConduitClosureInfo(Id);

            // Check if conduit is already attached to closure
            if (conduitClosureInfo.Sides.Exists(s => s.Ports.Exists(p => p.MultiConduitId == conduitId)))
                throw new ArgumentException("The conduit: " + conduitId + " is already attached to the closure: " + this.Id);

            if (conduitClosureInfo.Sides.Exists(s => s.Ports.Exists(p => p.Terminals.Exists(t => t.LineId == conduitId))))
                throw new ArgumentException("The conduit: " + conduitId + " is already attached to the closure: " + this.Id);

            // If ports already exists on the side and outgoing port argument not set, find next available port
            if (incommingPortPosition == 0 && conduitClosureInfo.Sides.Find(s => s.Position == incommingSide).Ports.Count != 0)
                incommingPortPosition = conduitClosureInfo.Sides.Find(s => s.Position == incommingSide).Ports.OrderByDescending(p => p.Position).First().Position + 1;
            else
                incommingPortPosition = 1; // No ports yet, so we put it on port 1


            // If ports already exists on the side and outgoing port argument not set, find next available port
            if (outgoingPortPosition == 0 && conduitClosureInfo.Sides.Find(s => s.Position == outgoingSide).Ports.Count != 0)
                outgoingPortPosition = conduitClosureInfo.Sides.Find(s => s.Position == outgoingSide).Ports.OrderByDescending(p => p.Position).First().Position + 1;
            else
                outgoingPortPosition = 1;  // No ports yet, so we put it on port 1

            // Check if port is the next in sequence.
            if (conduitClosureInfo.Sides.Find(s => s.Position == incommingSide).Ports.Count != 0)
            {
                if (conduitClosureInfo.Sides.Find(s => s.Position == incommingSide).Ports.OrderByDescending(p => p.Position).First().Position != (incommingPortPosition - 1))
                    throw new ArgumentException("Incomming port: " + incommingPortPosition + "on side: " + incommingSide + " is not the next number in sequence. Last port number used on side: " + incommingSide + " is: " + conduitClosureInfo.Sides.Find(s => s.Position == incommingSide).Ports.OrderByDescending(p => p.Position).First().Position);
            }
            else
            {
                if (incommingPortPosition != 1)
                    throw new ArgumentException("Incomming port: " + incommingPortPosition + "on side: " + incommingSide + " is not the next number in sequence. No ports currently added to side: " + incommingSide + ". Therefor port number = 1 was expected.");
            }

            if (conduitClosureInfo.Sides.Find(s => s.Position == outgoingSide).Ports.Count != 0)
            {
                if (conduitClosureInfo.Sides.Find(s => s.Position == outgoingSide).Ports.OrderByDescending(p => p.Position).First().Position != (outgoingPortPosition - 1))
                    throw new ArgumentException("Outgoing port: " + outgoingPortPosition + "on side: " + outgoingSide + " is not the next number in sequence. Last port number used on side: " + outgoingSide + " is: " + conduitClosureInfo.Sides.Find(s => s.Position == outgoingSide).Ports.OrderByDescending(p => p.Position).First().Position);
            }
            else
            {
                if (outgoingPortPosition != 1)
                    throw new ArgumentException("Outgoing port: " + outgoingPortPosition + "on side: " + outgoingSide + " is not the next number in sequence. No ports currently added to side: " + outgoingSide + ". Therefor port number = 1 was expected.");
            }

            ///////////////////////////////////////////////////////////////
            /// Finish checking. Now created the domain events
            var conduitAttached = new ConduitClosurePassingByConduitAttached()
            {
                ConduitClosureId = this.Id,
                ConduitId = conduitId,
                IncommingSide = incommingSide,
                OutgoingSide = outgoingSide,
                IncommingPortPosition = incommingPortPosition,
                OutgoingPortPosition = outgoingPortPosition,
                IncommingTerminalPosition = incommingTerminalPosition,
                OutgoingTerminalPosition = outgoingTerminalPosition
           };

           RaiseEvent(conduitAttached);
        }

        internal void AttachConduitEndToClosure(Guid conduitId, ConduitClosureSideEnum side, int portPosition, int terminalPosition, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkQueryService, IConduitClosureRepository conduitClosureRepository)
        {
            // Check if conduit is passing by closure
            var conduit = conduitNetworkQueryService.GetConduitInfo(conduitId);
            var walkOfInterest = routeNetworkQueryService.GetWalkOfInterestInfo(conduit.WalkOfInterestId);

            if (!walkOfInterest.AllNodeIds.Contains(_pointOfInterestId))
                throw new ArgumentException("Conduit: " + conduitId + " is not related to the point of interest: " + _pointOfInterestId + " where conduit closure: " + Id + " is placed at all.");

            if (!(walkOfInterest.StartNodeId == _pointOfInterestId || walkOfInterest.EndNodeId == _pointOfInterestId))
                throw new ArgumentException("Conduit: " + conduitId + " is not ending in point of interest: " + _pointOfInterestId + " - but not pasing through it. Please use AttachPassByConduitToClosure instead. The AttachConduitEndToClosure can only be used on conduits that are ending in the point of interest (route node) where the conduit closure is placed.");

            var conduitClosureInfo = conduitClosureRepository.GetConduitClosureInfo(Id);


            // Check if conduit is already attached to closure
            if (conduitClosureInfo.Sides.Exists(s => s.Ports.Exists(p => p.MultiConduitId == conduitId)))
                throw new ArgumentException("The conduit: " + conduitId + " is already attached to the closure: " + this.Id);

            if (conduitClosureInfo.Sides.Exists(s => s.Ports.Exists(p => p.Terminals.Exists(t => t.LineId == conduitId))))
                throw new ArgumentException("The conduit: " + conduitId + " is already attached to the closure: " + this.Id);



            // If ports already exists on the side and outgoing port argument not set, find next available port
            if (portPosition == 0 && conduitClosureInfo.Sides.Find(s => s.Position == side).Ports.Count != 0)
                portPosition = conduitClosureInfo.Sides.Find(s => s.Position == side).Ports.OrderByDescending(p => p.Position).First().Position + 1;
            else
                portPosition = 1; // No ports yet, so we put it on port 1

            // Check if port is the next in sequence.
            if (conduitClosureInfo.Sides.Find(s => s.Position == side).Ports.Count != 0)
            {
                if (conduitClosureInfo.Sides.Find(s => s.Position == side).Ports.OrderByDescending(p => p.Position).First().Position != (portPosition - 1))
                    throw new ArgumentException("Incomming port: " + portPosition + "on side: " + side + " is not the next number in sequence. Last port number used on side: " + side + " is: " + conduitClosureInfo.Sides.Find(s => s.Position == side).Ports.OrderByDescending(p => p.Position).First().Position);
            }
            else
            {
                if (portPosition != 1)
                    throw new ArgumentException("Incomming port: " + portPosition + "on side: " + side + " is not the next number in sequence. No ports currently added to side: " + side + ". Therefor port number = 1 was expected.");
            }

            // If a single conduit, and terminal not specified, find next avaiable terminal position
            if (terminalPosition == 0 && conduitClosureInfo.Sides.Exists(s => s.Position == side && s.Ports.Exists(p => p.Position == portPosition && p.Terminals.Count != 0)))
            {
                terminalPosition = conduitClosureInfo.
                    Sides.Find(s => s.Position == side).
                    Ports.Find(p => p.Position == portPosition).
                    Terminals.OrderByDescending(p => p.Position).First().Position + 1;
            }
            else
                terminalPosition = 1; // No terminals yet, so we put it on terminal 1


            ///////////////////////////////////////////////////////////////
            /// Finish checking. Now created the domain events
            var conduitEndAttached = new ConduitClosureConduitEndAttached()
            {
                ConduitClosureId = this.Id,
                ConduitId = conduitId,
                Side = side,
                PortPosition = portPosition,
                TerminalPosition = terminalPosition
            };

            RaiseEvent(conduitEndAttached);
        }


        private void Apply(ConduitClosurePlaced @event)
        {
            Id = @event.ConduitClosureId;
            _pointOfInterestId = @event.PointOfInterestId;
        }

        private void Apply(ConduitClosureRemoved @event)
        {
            _removed = true;
        }

        private void Apply(ConduitClosurePassingByConduitAttached @event)
        {
        }

        private void Apply(ConduitClosureConduitEndAttached @event)
        {
        }

    }
}
