using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Events
{
    public class MultiConduitOuterConduitCut
    {
        public Guid MultiConduitId { get; set; }
        public Guid PointOfInterestId { get; set; }
        public MultiConduitCutKindEnum CutKind { get; set; }
    }
}
