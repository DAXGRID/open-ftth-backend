using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// A conduit closure has been removed, from the point of interest where it previously was placed
    /// </summary>
    public class ConduitClosureRemoved
    {
        public Guid ConduitClosureId { get; set; }
    }
}
