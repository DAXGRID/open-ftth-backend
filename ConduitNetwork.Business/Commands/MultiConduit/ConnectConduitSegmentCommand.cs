using ConduitNetwork.Events.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class ConnectConduitSegmentCommand : IRequest
    {
        public Guid PointOfInterestId { get; set; }
        public Guid FromConduitSegmentId { get; set; }
        public Guid ToConduitSegmentId { get; set; }
    }
}
