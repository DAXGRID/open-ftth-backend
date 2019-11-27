using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public class SegmentWithRouteNodeRelationInfo
    {
        public Guid RouteNodeId { get; set; }
        public ISegment Segment { get; set; }
        public SegmentRelationTypeEnum RelationType { get; set; }

        public override string ToString()
        {
            string result = Segment.Line.LineKind.ToString() + " Segment: " + Segment.Id;
            return result;
        }
    }
}
