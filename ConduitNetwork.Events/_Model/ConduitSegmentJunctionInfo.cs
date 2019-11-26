using Core.GraphSupport.Model;
using Core.ReadModel.Network;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ConduitNetwork.Events.Model
{
    public class ConduitSegmentJunctionInfo : GraphNode, INode
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        [IgnoreDataMember]
        private List<ConduitSegmentInfo> FromConduitSegments { get; set; }

        public void AddFromConduitSegment(ConduitSegmentInfo segment)
        {
            if (ToConduitSegments == null)
                ToConduitSegments = new List<ConduitSegmentInfo>();

            ToConduitSegments.Add(segment);
        }

        public override List<IGraphElement> IngoingElements
        {
            get
            {
                List<IGraphElement> result = new List<IGraphElement>();

                if (FromConduitSegments != null)
                    result.AddRange(FromConduitSegments);

                return result;
            }
        }



        [IgnoreDataMember]
        private List<ConduitSegmentInfo> ToConduitSegments { get; set; }

        public void AddToConduitSegment(ConduitSegmentInfo segment)
        {
            if (ToConduitSegments == null)
                ToConduitSegments = new List<ConduitSegmentInfo>();

            ToConduitSegments.Add(segment);
        }

        public override List<IGraphElement> OutgoingElements
        {
            get
            {
                /*
                if (ToConduitSegment != null)
                    return new List<IGraphElement>() { ToConduitSegment };
                else
                    return new List<IGraphElement>();
                    */
                List<IGraphElement> result = new List<IGraphElement>();

                if (FromConduitSegments != null)
                    result.AddRange(ToConduitSegments);

                return result;
            }
        }
        public override List<IGraphElement> NeighborElements
        {
            get
            {
                List<IGraphElement> result = new List<IGraphElement>();

                if (FromConduitSegments != null)
                    result.AddRange(FromConduitSegments);

                if (ToConduitSegments != null)
                    result.AddRange(ToConduitSegments);

                return result;
            }
        }
    }
}
