using Infrastructure.EventSourcing;
using RouteNetwork.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Business.Aggregates
{
    public class WalkOfInterest : AggregateBase
    {
        private WalkOfInterest()
        {
            // Register the event types that make up our aggregate , together with their respective handlers
            Register<WalkOfInterestRegistered>(Apply);
        }

        internal WalkOfInterest(Guid walkOfInterestId, List<Guid> routeElementIds) : this()
        {
            // Create domain event
            var walkOfInterestRegistered = new WalkOfInterestRegistered()
            {
                Id = walkOfInterestId,
                RouteElementIds = routeElementIds
            };

            RaiseEvent(walkOfInterestRegistered);
        }

        private void Apply(WalkOfInterestRegistered @event)
        {
            Id = @event.Id;
        }
    }
}
