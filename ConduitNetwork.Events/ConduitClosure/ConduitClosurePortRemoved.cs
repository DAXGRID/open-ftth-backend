using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// A port has been removed from the specified side of the conduit closure
    /// </summary>
    public class ConduitClosurePortRemoved
    {
        public Guid ConduitClosureId { get; set; }
        public int SideNumber { get; set; }
        public int PortNumber { get; set; }
    }
}
