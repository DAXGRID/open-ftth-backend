using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class RemoveConduitClosureCommand : IRequest
    {
        public Guid ConduitClosureId { get; set; }
    }
}
