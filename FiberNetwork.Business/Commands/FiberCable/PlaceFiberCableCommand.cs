using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiberNetwork.Business.Commands
{
    public class PlaceFiberCableCommand : IRequest
    {
        public Guid FiberCableId { get; set; }
        public Guid WalkOfInterestId { get; set; }
        public string Name { get; set; }
        public int NumberOfFibers { get; set; }
    }
}
