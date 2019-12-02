using ConduitNetwork.Events.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class ConnectInnerConduitToJunction : IRequest
    {
        public Guid MultiConduitId { get; set; }
        public Guid PointOfInterestId { get; set; }
        public int InnerConduitSequenceNumber { get; set; }
        public ConduitEndKindEnum ConnectedEndKind { get; set; }
        public Guid ConnectedJunctionId { get; set; }
    }
}
