using ConduitNetwork.Events.Model;
using ConduitNetwork.ReadModel;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Projections
{
    public static class ConduitCutter
    {

        public static void CutConduit(ConduitInfo conduitInfo, WalkOfInterestInfo walkOfInterest, RouteNodeInfo nodeWhereToCut)
        {
            ConduitSegmentInfo newSegment = null;

            List<ConduitSegmentInfo> newSegmentList = new List<ConduitSegmentInfo>();

            int newSequenceNumber = 1;

            var fromNodeId = conduitInfo.Segments[0].FromNodeId;

            foreach (var existingSegment in conduitInfo.Segments)
            {
                List<Guid> segmentWalk = walkOfInterest.SubWalk(existingSegment.FromNodeId, existingSegment.ToNodeId);

                newSegmentList.Add(existingSegment);
                existingSegment.SequenceNumber = newSequenceNumber;
                existingSegment.FromNodeId = fromNodeId;

                // If the segment is cut by point of interest, divide it
                if (segmentWalk.Contains(nodeWhereToCut.Id))
                {
                    // Create the segment
                    newSequenceNumber++;

                    if (conduitInfo.Kind != ConduitKindEnum.MultiConduit)
                        newSegment = new SingleConduitSegmentInfo();
                    else
                        newSegment = new MultiConduitSegmentInfo();

                    newSegment.Id = Guid.NewGuid();
                    newSegment.ConduitId = existingSegment.ConduitId;
                    newSegment.SequenceNumber = newSequenceNumber;
                    newSegment.FromNodeId = nodeWhereToCut.Id;
                    newSegment.ToNodeId = existingSegment.ToNodeId;  // we need copy to side info
                    newSegment.ToJunctionId = existingSegment.ToJunctionId; // we need copy to side info
                    newSegment.ToJunction = existingSegment.ToJunction; // we need copy to side info

                    // Update the existing segment
                    existingSegment.ToNodeId = nodeWhereToCut.Id;
                    existingSegment.ToJunctionId = Guid.Empty; // cannot possible have to junction anymore if it had so (transfered to new segment)
                    existingSegment.ToJunction = null; // cannot possible have to junction anymore if it had so (transfered to new segment)

                    // Set from node on next segment to from node on inserted segment
                    fromNodeId = newSegment.ToNodeId;

                    newSegmentList.Add(newSegment);
                }
                else
                {
                    // set from node to this one to node
                    fromNodeId = existingSegment.ToNodeId;
                }

                newSequenceNumber++;
            }

            conduitInfo.Segments = newSegmentList;

            // Needed to wake up Marten
            conduitInfo.Name = conduitInfo.Name;
        }

    }
}
