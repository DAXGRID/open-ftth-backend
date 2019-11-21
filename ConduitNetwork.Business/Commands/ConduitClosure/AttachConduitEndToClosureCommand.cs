using ConduitNetwork.Events.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class AttachConduitEndToClosureCommand : IRequest
    {
        public Guid ConduitClosureId { get; set; }
        public Guid ConduitId { get; set; }
        public ConduitClosureInfoSide Side { get; set; }
        public int PortPosition { get; set; }
        public int TerminalPosition { get; set; }
    }
}
