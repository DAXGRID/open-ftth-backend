using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class CutInnerConduitCommand : IRequest
    {
        public Guid MultiConduitId { get; set; }
        public Guid PointOfInterestId { get; set; }
        public int InnerConduitSequenceNumber { get; set; }
        public Guid InnerConduitId { get; set; }
    }
}
