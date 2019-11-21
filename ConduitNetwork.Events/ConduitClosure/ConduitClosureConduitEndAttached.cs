using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// A conduit end has been attached to a conduit closure
    /// </summary>
    public class ConduitClosureConduitEndAttached
    {
        public Guid ConduitClosureId { get; set; }
        public Guid ConduitId { get; set; }
        public ConduitClosureInfoSide Side { get; set; }
        public int PortPosition { get; set; }
        public int TerminalPosition { get; set; }
    }
}
