using Asset.Model;
using System;

namespace FiberNetwork.Events
{
    public class FiberCablePlaced
    {
        public Guid FiberCableId { get; set; }
        public Guid WalkOfInterestId { get; set; }
        public int NumberOfFibers { get; set; }
        public AssetInfo AssetInfo { get; set; }
    }
}
