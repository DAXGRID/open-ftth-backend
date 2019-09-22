using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Events
{
    public class MultiConduitInnerConduitCut
    {
        public Guid MultiConduitId { get; set; }
        public Guid PointOfInterestId { get; set; }
        public int InnerConduitSequenceNumber { get; set; }
    }
}
