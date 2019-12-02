using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class CutOuterConduitCommand : IRequest
    {
        public Guid MultiConduitId { get; set; }
        public Guid PointOfInterestId { get; set; }
    }
}
