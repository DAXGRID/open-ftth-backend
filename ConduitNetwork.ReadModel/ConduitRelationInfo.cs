using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.ReadModel
{
    public class ConduitRelationInfo
    {
        public ConduitRelationTypeEnum Type { get; set; }

        public ConduitSegmentInfo Segment { get; set; }

        public override string ToString()
        {
            string result = this.Type.ToString();

            result += " -> " + Segment.ToString();

            return result; ;
        }
    }
}
