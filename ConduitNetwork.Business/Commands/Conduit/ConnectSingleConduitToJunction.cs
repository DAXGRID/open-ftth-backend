using ConduitNetwork.Events.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class ConnectSingleConduitToJunction : IRequest
    {
        public Guid SingleConduitId { get; set; }
        public Guid PointOfInterestId { get; set; }
        public ConduitEndKindEnum ConnectedEndKind { get; set; }
        public Guid ConnectedJunctionId { get; set; }
    }
}
