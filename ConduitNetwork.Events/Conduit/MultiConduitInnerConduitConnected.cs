using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Events
{
    public class MultiConduitInnerConduitConnected
    {
        public Guid MultiConduitId { get; set; }
        public Guid PointOfInterestId { get; set; }
        public int InnerConduitSequenceNumber { get; set; }
        public ConduitEndKindEnum ConnectedEndKind { get; set; }
        public Guid ConnectedJunctionId { get; set; }
    }
}
