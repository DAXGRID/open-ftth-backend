using Location.Model;
using MediatR;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Business.Commands
{
    public class SplitSegmentCommand : IRequest
    {
        public Guid SegmentId { get; set; }

        public Guid NodeId { get; set; }
    }
}
