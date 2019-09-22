using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// A multi conduit end has been attached to a conduit closure port. 
    /// That is, the incomming or outgoing end/part of a multi conduit is now attached to the conduit closure side/port specified.
    /// </summary>
    public class MultiConduitEndAttachedToConduitClosurePort
    {
        public Guid ConduitClosureId { get; set; }
        public Guid MultiConduitId { get; set; }
        public ConduitEndKindEnum ConduitEndKind { get; set; }
        public int SideNumber { get; set; }
        public int PortNumber { get; set; }
    }
}
