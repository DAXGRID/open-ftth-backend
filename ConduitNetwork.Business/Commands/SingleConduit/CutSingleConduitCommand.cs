using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class CutSingleConduitCommand : IRequest
    {
        public Guid SingleConduitId { get; set; }
        public Guid PointOfInterestId { get; set; }
    }
}
