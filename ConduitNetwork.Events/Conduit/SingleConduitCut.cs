using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Events
{
    public class SingleConduitCut
    {
        public Guid SingleConduitId { get; set; }
        public Guid PointOfInterestId { get; set; }
    }
}
