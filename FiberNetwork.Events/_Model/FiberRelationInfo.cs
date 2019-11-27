using Core.ReadModel.Network;
using FiberNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiberNetwork.Events.Model
{
    public class FiberRelationInfo : ISegmentRelation
    {
        public SegmentRelationTypeEnum Type { get; set; }

        public ISegment Segment { get; set; }

        public override string ToString()
        {
            string result = this.Type.ToString();

            result += " -> " + Segment.ToString();

            return result; ;
        }
    }
}
