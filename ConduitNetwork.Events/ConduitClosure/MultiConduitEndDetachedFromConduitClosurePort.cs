using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// A multi conduit end has been detached from a conduit closure port. 
    /// That is, the multi conduit end is now detached from the conduit closure and is just ending in or passing through the point of interest (node), having no relationship to the conduit closure anymore.
    /// </summary>
    public class MultiConduitEndDetachedFromConduitClosurePort
    {
        public Guid ConduitClosureId { get; set; }
        public Guid MultiConduitId { get; set; }
        public ConduitEndKindEnum ConduitEndKind { get; set; }
    }
}
