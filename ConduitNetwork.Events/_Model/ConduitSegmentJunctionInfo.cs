using Core.GraphSupport.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ConduitNetwork.Events.Model
{
    public class ConduitSegmentJunctionInfo : GraphNode
    {
        public Guid Id { get; set; }
        [IgnoreDataMember]
        public ConduitSegmentInfo FromConduitSegment { get; set; }
        public override List<IGraphElement> IngoingElements
        {
            get
            {
                if (FromConduitSegment != null)
                    return new List<IGraphElement>() { FromConduitSegment };
                else
                    return new List<IGraphElement>();
            }
        }

        [IgnoreDataMember]
        public ConduitSegmentInfo ToConduitSegment { get; set; }
        public override List<IGraphElement> OutgoingElements
        {
            get
            {
                if (ToConduitSegment != null)
                    return new List<IGraphElement>() { ToConduitSegment };
                else
                    return new List<IGraphElement>();
            }
        }
        public override List<IGraphElement> NeighborElements
        {
            get
            {
                List<IGraphElement> result = new List<IGraphElement>();

                if (FromConduitSegment != null)
                    result.Add(FromConduitSegment);

                if (ToConduitSegment != null)
                    result.Add(ToConduitSegment);

                return result;
            }
        }
    }
}
