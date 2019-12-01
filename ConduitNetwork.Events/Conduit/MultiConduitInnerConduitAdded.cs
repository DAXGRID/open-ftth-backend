using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Events
{
    public class MultiConduitInnerConduitAdded
    {
        public Guid MultiConduitId { get; set; }
        public int MultiConduitIndex { get; set; }
        public ConduitInfo ConduitInfo { get; set; }
    }
}
