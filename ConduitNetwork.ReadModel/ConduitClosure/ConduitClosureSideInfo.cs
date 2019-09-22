using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.ReadModel.ConduitClosure
{
    public class ConduitClosureSideInfo
    {
        public ConduitClosureSideEnum Position { get; set; }
        public string DigramLabel { get; set; }
        public List<ConduitClosurePortInfo> Ports { get; set; }
    }
}
