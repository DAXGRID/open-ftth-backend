using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public interface ISegmentRelation
    {
      SegmentRelationTypeEnum Type { get; set; }
      ISegment Segment { get; set; }
    }
}
