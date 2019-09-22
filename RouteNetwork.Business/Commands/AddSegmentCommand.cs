using MediatR;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Business.Commands
{
    public class AddSegmentCommand : IRequest
    {
        public Guid Id { get; set; }
        public Guid FromNodeId { get; set; }
        public Guid ToNodeId { get; set; }
        public RouteSegmentKindEnum SegmentKind { get; set; }
        public Geometry Geometry { get; set; }
    }
}
