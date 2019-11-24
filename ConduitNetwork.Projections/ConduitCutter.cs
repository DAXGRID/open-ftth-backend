using ConduitNetwork.Events.Model;
using ConduitNetwork.ReadModel;
using Core.ReadModel.Network;
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

            List<ILineSegment> newSegmentList = new List<ILineSegment>();

            int newSequenceNumber = 1;

            var fromNodeId = conduitInfo.Segments[0].FromRouteNodeId;

            foreach (var existingSegment in conduitInfo.Segments)
            {
                List<Guid> segmentWalk = walkOfInterest.SubWalk(existingSegment.FromRouteNodeId, existingSegment.ToRouteNodeId);

                newSegmentList.Add(existingSegment);
                existingSegment.SequenceNumber = newSequenceNumber;
                existingSegment.FromRouteNodeId = fromNodeId;

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
                    newSegment.ConduitId = ((ConduitSegmentInfo)existingSegment).ConduitId;
                    newSegment.SequenceNumber = newSequenceNumber;
                    newSegment.FromRouteNodeId = nodeWhereToCut.Id;
                    newSegment.ToRouteNodeId = existingSegment.ToRouteNodeId;  // we need copy to side info
                    newSegment.ToNodeId = existingSegment.ToNodeId; // we need copy to side info
                    newSegment.ToNode = existingSegment.ToNode; // we need copy to side info

                    // Update the existing segment
                    existingSegment.ToRouteNodeId = nodeWhereToCut.Id;
                    existingSegment.ToNodeId = Guid.Empty; // cannot possible have to junction anymore if it had so (transfered to new segment)
                    existingSegment.ToNode = null; // cannot possible have to junction anymore if it had so (transfered to new segment)

                    // Set from node on next segment to from node on inserted segment
                    fromNodeId = newSegment.ToRouteNodeId;

                    newSegmentList.Add(newSegment);
                }
                else
                {
                    // set from node to this one to node
                    fromNodeId = existingSegment.ToRouteNodeId;
                }

                newSequenceNumber++;
            }

            conduitInfo.Segments = newSegmentList;

            // Needed to wake up Marten
            conduitInfo.Name = conduitInfo.Name;
        }

    }
}
