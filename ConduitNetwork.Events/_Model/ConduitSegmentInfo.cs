using Core.GraphSupport.Model;
using Core.ReadModel.Network;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ConduitNetwork.Events.Model
{
    public class ConduitSegmentInfo : GraphEdge, ILineSegment
    {
        public Guid Id { get; set; }
        public Guid ConduitId { get; set; }
        public int SequenceNumber { get; set; }
        public Guid FromNodeId { get; set; }
        public Guid ToNodeId { get; set; }
        public Guid FromJunctionId { get; set; }
        public Guid ToJunctionId { get; set; }


        #region Properties that should not be persisted

        [IgnoreDataMember]
        public ConduitInfo Conduit { get; set; }

        [IgnoreDataMember]
        public List<ConduitSegmentInfo> Parents { get; set; }

        [IgnoreDataMember]
        public List<ConduitSegmentInfo> Children { get; set; }

        [IgnoreDataMember]
        public ConduitSegmentJunctionInfo FromJunction { get; set; }

        [IgnoreDataMember]
        public ConduitSegmentJunctionInfo ToJunction { get; set; }

        #endregion

        public LineSegmentKindEnum LineSegmentKind {
            get
            {
                return LineSegmentKindEnum.Conduit;
            }
        }


        public override List<IGraphElement> IngoingElements
        {
            get
            {
                if (FromJunction != null)
                    return new List<IGraphElement>() { FromJunction };
                else
                    return new List<IGraphElement>();
            }
        }

        public override List<IGraphElement> OutgoingElements
        {
            get
            {
                if (ToJunction != null)
                    return new List<IGraphElement>() { ToJunction };
                else
                    return new List<IGraphElement>();
            }
        }
        public override List<IGraphElement> NeighborElements
        {
            get
            {
                List<IGraphElement> result = new List<IGraphElement>();

                if (FromJunction != null)
                    result.Add(FromJunction);

                if (ToJunction != null)
                    result.Add(ToJunction);

                return result;
            }
        }

        public override string ToString()
        {
            string result = SequenceNumber + " ";

            if (Conduit != null)
            {
                result += " -> " + Conduit.ToString();
            }

            return result;
        }
    }
}
