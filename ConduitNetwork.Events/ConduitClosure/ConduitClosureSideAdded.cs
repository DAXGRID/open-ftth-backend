using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// A side has been added to the specified closure
    /// </summary>
    public class ConduitClosureSideAdded
    {
        public Guid ConduitClosureId { get; set; }
        public int SideNumber { get; set; }
    }
}
