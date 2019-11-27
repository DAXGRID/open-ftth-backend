using Core.ReadModel.Network;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FiberNetwork.Events.Model
{
    public abstract class FiberInfo : ILine
    {
        public Guid Id { get; set; }
        public Guid WalkOfInterestId { get; set; }
        public string Name { get; set; }
        public int SequenceNumber { get; set; }
        public List<ISegment> Segments { get; set; }
        public List<ILine> Children { get; set; }

        #region Properties that should not be persisted

        [IgnoreDataMember]
        public INode FromRouteNode { get; set; }

        [IgnoreDataMember]
        public INode ToRouteNode { get; set; }

        [IgnoreDataMember]
        public ILine Parent { get; set; }

        #endregion

        public abstract LineKindEnum LineKind { get; }

        public ILine GetRoot()
        {
            if (Parent != null)
                return (ILine)Parent;
            else
                return this;
        }
    }
}
