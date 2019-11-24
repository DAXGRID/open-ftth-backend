using System;
using System.Collections.Generic;
using System.Text;

namespace FiberNetwork.Events
{
    public class FiberConnected
    {
        public Guid FiberCableId { get; set; }
        public Guid PointOfInterestId { get; set; }
        public int FiberSequenceNumber { get; set; }
        //public ConduitEndKindEnum ConnectedEndKind { get; set; }
        public Guid ConnectedNodeId { get; set; }
    }
}
