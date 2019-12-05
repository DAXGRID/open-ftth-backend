using ConduitNetwork.Events.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class AddCableToConduitCommand : IRequest
    {
        public Guid FiberCableSegmentId { get; set; }
        public Guid ConduitSegmentSegmentId1 { get; set; }
        public Guid ConduitSegmentSegmentId2 { get; set; }
    }
}
