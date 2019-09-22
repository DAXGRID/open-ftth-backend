using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// A port has been added to the specified side of the conduit closure
    /// </summary>
    public class ConduitClosurePortAdded
    {
        public Guid ConduitClosureId { get; set; }
        public int SideNumber { get; set; }
        public int PortNumber { get; set; }
    }
}
