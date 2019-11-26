using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public interface ILineSegmentRelation
    {
      LineSegmentRelationTypeEnum Type { get; set; }
      ILineSegment Segment { get; set; }
    }
}
