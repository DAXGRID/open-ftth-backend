using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// A single conduit end has been attached to a conduit closure terminal. 
    /// That is, the incomming or outgoing end of the single conduit is now attached to the conduit closure side/port/terminal specified.
    /// Notice that the difference between this event and the MultiConduitEndAttachedToConduitClosurePort is that single conduits are attached to terminals, whereas multi conduits are attached to ports.
    /// </summary>
    public class SingleConduitEndAttachedToConduitClosureTerminal
    {
        public Guid ConduitClosureId { get; set; }
        public Guid MultiConduitId { get; set; }
        public Guid SingleConduitId { get; set; }
        public ConduitEndKindEnum ConduitEndKind { get; set; }
        public int SideNumber { get; set; }
        public int PortNumber { get; set; }
        public int TerminalNumber { get; set; }
        public TerminalConnectionKindEnum TerminalConnectionKind { get; set; }
    }
}
