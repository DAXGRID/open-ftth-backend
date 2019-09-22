using Asset.Model;
using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ConduitNetwork.ReadModel.ConduitClosure
{
    public class ConduitClosureInfo
    {
        public Guid Id { get; set; }
        public Guid PointOfInterestId { get; set; }
        public List<ConduitClosureSideInfo> Sides { get; set; }
    }
}
