using MediatR;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Business.Commands
{
    public class RegisterWalkOfInterestCommand : IRequest
    {
        public Guid WalkOfInterestId { get; set; }

        public List<Guid> RouteElementIds { get; set; }
    }
}
