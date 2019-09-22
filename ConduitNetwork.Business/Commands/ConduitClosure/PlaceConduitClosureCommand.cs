using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class PlaceConduitClosureCommand : IRequest
    {
        public Guid ConduitClosureId { get; set; }
        public Guid PointOfInterestId { get; set; }
    }
}
