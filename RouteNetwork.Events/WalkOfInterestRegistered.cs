using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Events
{
    public class WalkOfInterestRegistered
    {
        public Guid Id { get; set; }

        public List<Guid> RouteElementIds { get; set; }
    }
}
