using ConduitNetwork.Events.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.Commands
{
    public class PlaceSingleConduitCommand : IRequest
    {
        public Guid SingleConduitId { get; set; }
        public Guid WalkOfInterestId { get; set; }
        public string Name { get; set; }
        public ConduitColorEnum MarkingColor { get; set; }
        public string MarkingText { get; set; }
        public Guid ConduitSpecificationId { get; set; }

        /// <summary>
        /// Must die
        /// </summary>
        public string DemoDataSpec { get; set; }
    }
}
