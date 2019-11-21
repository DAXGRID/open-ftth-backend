using ConduitNetwork.Events.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class AttachPassByConduitToClosureCommand : IRequest
    {
        public Guid ConduitClosureId { get; set; }
        public Guid ConduitId { get; set; }
        public ConduitClosureInfoSide IncommingSide { get; set; }
        public ConduitClosureInfoSide OutgoingSide { get; set; }
        public int IncommingPortPosition { get; set; }
        public int OutgoingPortPosition { get; set; }
        public int IncommingTerminalPosition { get; set; }
        public int OutgoingTerminalPosition { get; set; }

    }
}
