using Location.Model;
using MediatR;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Business.Commands
{
    public class AddNodeCommand : IRequest
    {
        public Guid Id { get; set; }

        public String Name { get; set; }

        public RouteNodeKindEnum NodeKind { get; set; }

        public RouteNodeFunctionKindEnum NodeFunctionKind { get; set; }

        public Geometry Geometry { get; set; }

        public LocationInfo LocationInfo { get; set; }
    }
}
