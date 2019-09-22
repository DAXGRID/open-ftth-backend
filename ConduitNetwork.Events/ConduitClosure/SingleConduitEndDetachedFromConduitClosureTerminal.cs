using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// A single conduit end has been detached from a conduit closure terminal. 
    /// That is, the incomming or outgoing end of the single conduit is now detached from the conduit closure.
    /// </summary>
    public class SingleConduitEndDetachedFromConduitClosureTerminal
    {
        public Guid ConduitClosureId { get; set; }
        public Guid MultiConduitId { get; set; }
        public Guid SingleConduitId { get; set; }
        public ConduitEndKindEnum ConduitEndKind { get; set; }
    }
}
