using Core.GraphSupport.Model;
using RouteNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace RouteNetwork.ReadModel
{
    public sealed class WalkOfInterestInfo
    {
        public Guid Id { get; set; }

        public List<Guid> RouteElementIds { get; set; }

        [IgnoreDataMember]
        public List<GraphElement> RouteElements { get; set; }

        [IgnoreDataMember]
        public Guid StartNodeId
        {
            get
            {
                return RouteElementIds[0];
            }
        }

        [IgnoreDataMember]
        public Guid EndNodeId
        {
            get
            {
                return RouteElementIds[RouteElementIds.Count - 1];
            }
        }

        [IgnoreDataMember]
        public Guid StartSegmentId
        {
            get
            {
                return RouteElementIds[1];
            }
        }

        [IgnoreDataMember]
        public Guid EndSegmentId
        {
            get
            {
                return RouteElementIds[RouteElementIds.Count - 2];
            }
        }

        [IgnoreDataMember]
        public List<Guid> AllNodeIds
        {
            get
            {
                List<Guid> result = new List<Guid>();

                for (int i = 0; i < RouteElementIds.Count; i+=2)
                {
                    result.Add(RouteElementIds[i]);
                }

                return result;
            }
        }

        [IgnoreDataMember]
        public List<Guid> AllSegmentIds
        {
            get
            {
                List<Guid> result = new List<Guid>();

                for (int i = 1; i < (RouteElementIds.Count - 1); i+=2)
                {
                    result.Add(RouteElementIds[i]);
                }

                return result;
            }
        }


        /// <summary>
        /// Helper function to extract a sub walk of a walk
        /// </summary>
        /// <param name="fromNodeId"></param>
        /// <param name="toNodeId"></param>
        /// <returns></returns>
        public List<Guid> SubWalk(Guid fromNodeId, Guid toNodeId)
        {
            List<Guid> result = new List<Guid>();

            bool subWalkInProgress = false;

            foreach (var routeElementId in RouteElementIds)
            {
                if (subWalkInProgress)
                {
                    result.Add(routeElementId);

                    if (routeElementId == fromNodeId || routeElementId == toNodeId)
                        subWalkInProgress = false;
                }
                else if (routeElementId == fromNodeId || routeElementId == toNodeId)
                {
                    result.Add(routeElementId);
                    subWalkInProgress = true;
                }
            }

            if (subWalkInProgress)
                throw new ArgumentException("Never found toNodeId: " + toNodeId);

            return result;
        }

        public WalkOfInterestInfo SubWalk2(Guid fromNodeId, Guid toNodeId)
        {
            WalkOfInterestInfo result = new WalkOfInterestInfo();
            result.RouteElementIds = new List<Guid>();

            bool subWalkInProgress = false;

            foreach (var routeElementId in RouteElementIds)
            {
                if (subWalkInProgress)
                {
                    result.RouteElementIds.Add(routeElementId);

                    if (routeElementId == fromNodeId || routeElementId == toNodeId)
                        subWalkInProgress = false;
                }
                else if (routeElementId == fromNodeId || routeElementId == toNodeId)
                {
                    result.RouteElementIds.Add(routeElementId);
                    subWalkInProgress = true;
                }
            }

            if (subWalkInProgress)
                throw new ArgumentException("Never found toNodeId: " + toNodeId);

            return result;
        }
    }

}
