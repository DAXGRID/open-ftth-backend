using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ConduitNetwork.ReadModel.ConduitClosure
{
    public class ConduitClosurePortInfo
    {
        public ConduitClosurePortInfo()
        {
            Terminals = new List<ConduitClosureTerminalInfo>();
        }

        public int Position { get; set; }
        public string DiagramLabel { get; set; }
        public Guid MultiConduitId { get; set; }
        public Guid MultiConduitSegmentId { get; set; }
        public ConduitEndKindEnum MultiConduitSegmentEndKind { get; set; }
        public ConduitClosureInternalConnectionKindEnum ConnectionKind { get; set; }
        public ConduitClosureInfoSide ConnectedToSide { get; set; }
        public int ConnectedToPort { get; set; }
        public List<ConduitClosureTerminalInfo> Terminals { get; set; }

        [IgnoreDataMember]
        public ConduitSegmentInfo MultiConduitSegment { get; set; }
    }
}
