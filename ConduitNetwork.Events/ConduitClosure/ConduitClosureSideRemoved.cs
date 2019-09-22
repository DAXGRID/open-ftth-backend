using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// A side has been removed from the specified closure
    /// </summary>
    public class ConduitClosureSideRemoved
    {
        public Guid ConduitClosureId { get; set; }
        public int SideNumber { get; set; }
    }
}
