using Core.ReadModel.Network;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FiberNetwork.Events.Model
{
    public class FiberInfo : ILine
    {
        public Guid Id { get; set; }
        public Guid WalkOfInterestId { get; set; }
        public string Name { get; set; }
        public int SequenceNumber { get; set; }
        public List<ILineSegment> Segments { get; set; }

        #region Properties that should not be persisted

        [IgnoreDataMember]
        public List<ILine> Children { get; set; }

        [IgnoreDataMember]
        public ILine Parent { get; set; }

        #endregion

        public LineKindEnum LineKind
        {
            get
            {
                return LineKindEnum.SignalFiberCable;
            }
        }

        public ILine GetRoot()
        {
            if (Parent != null)
                return (ILine)Parent;
            else
                return this;
        }
    }
}
