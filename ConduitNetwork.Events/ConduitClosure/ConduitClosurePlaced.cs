using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// A conduit closure has been placed in a point of interest (route node)
    /// </summary>
    public class ConduitClosurePlaced
    {
        public Guid ConduitClosureId { get; set; }
        public Guid PointOfInterestId { get; set; }
    }
}
