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
        public Guid ConduitId { get; set; }
        public int SequenceNumber { get; set; }
        public Guid FromRouteNodeId { get; set; }
        public Guid ToRouteNodeId { get; set; }
        public Guid FromNodeId { get; set; }
        public Guid ToNodeId { get; set; }


        #region Properties that should not be persisted

        [IgnoreDataMember]
        public ConduitInfo Conduit { get; set; }

        [IgnoreDataMember]
        public ILine Line
        {
            get { return Conduit; }
            set { Conduit = (ConduitInfo)value; }
        }

        [IgnoreDataMember]
        public List<ILineSegment> Parents { get; set; }

        [IgnoreDataMember]
        public List<ILineSegment> Children { get; set; }

        [IgnoreDataMember]
        public INode FromRouteNode { get; set; }

        [IgnoreDataMember]
        public INode ToRouteNode { get; set; }

        [IgnoreDataMember]
        public INode FromNode { get; set; }

        [IgnoreDataMember]
        public INode ToNode { get; set; }

        #endregion

        public override List<IGraphElement> IngoingElements
        {
            get
            {
                if (FromNode != null)
                    return new List<IGraphElement>() { FromNode };
                else
                    return new List<IGraphElement>();
            }
        }

        public override List<IGraphElement> OutgoingElements
        {
            get
            {
                if (ToNode != null)
                    return new List<IGraphElement>() { ToNode };
                else
                    return new List<IGraphElement>();
            }
        }
        public override List<IGraphElement> NeighborElements
        {
            get
            {
                List<IGraphElement> result = new List<IGraphElement>();

                if (FromNode != null)
                    result.Add(FromNode);

                if (ToNode != null)
                    result.Add(ToNode);

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
