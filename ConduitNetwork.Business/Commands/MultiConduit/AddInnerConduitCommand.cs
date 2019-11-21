using ConduitNetwork.Events.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class AddInnerConduitCommand : IRequest
    {
        public Guid MultiConduitId { get; set; }
        public ConduitColorEnum Color { get; set; }
        public int OuterDiameter { get; set; }
        public int InnerDiameter { get; set; }
    }
}
