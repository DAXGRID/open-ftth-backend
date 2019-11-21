using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// A conduit has been routed through a conduit closure
    /// </summary>
    public class ConduitClosurePassingByConduitAttached
    {
        public Guid ConduitClosureId { get; set; }
        public Guid ConduitId { get; set; }
        public ConduitClosureInfoSide IncommingSide { get; set; }
        public ConduitClosureInfoSide OutgoingSide { get; set; }
        public int IncommingPortPosition { get; set; }
        public int OutgoingPortPosition { get; set; }
        public int IncommingTerminalPosition { get; set; }
        public int OutgoingTerminalPosition { get; set; }
    }
}
