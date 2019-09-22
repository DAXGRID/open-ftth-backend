using Asset.Model;
using ConduitNetwork.Events.Model;
using System;

namespace ConduitNetwork.Events
{
    public class SingleConduitPlaced
    {
        public Guid SingleConduitId { get; set; }
        public Guid WalkOfInterestId { get; set; }
        public ConduitInfo ConduitInfo { get; set; }
        public AssetInfo AssetInfo { get; set; }
    }
}
