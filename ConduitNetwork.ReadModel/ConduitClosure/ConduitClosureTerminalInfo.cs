using ConduitNetwork.Events.Model;
using Core.ReadModel.Network;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ConduitNetwork.ReadModel.ConduitClosure
{
    public class ConduitClosureTerminalInfo
    {
        public int Position { get; set; }
        public string DiagramLabel { get; set; }
        public Guid LineId { get; set; }
        public Guid LineSegmentId { get; set; }
        public ConduitEndKindEnum LineSegmentEndKind { get; set; }
        public ConduitClosureInternalConnectionKindEnum ConnectionKind { get; set; }
        public ConduitClosureInfoSide ConnectedToSide { get; set; }
        public int ConnectedToPort { get; set; }
        public int ConnectedToTerminal { get; set; }

        [IgnoreDataMember]
        public ILineSegment LineSegment { get; set; }
    }
}
