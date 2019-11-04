using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;

namespace RouteNetwork.QueryService
{
    public interface IRouteNetworkState
    {
        bool CheckIfRouteNodeIdExists(Guid id);
        bool CheckIfRouteSegmentIdExists(Guid id);
        IRouteElementInfo GetRouteElementInfo(Guid id);
        RouteNodeInfo GetRouteNodeInfo(Guid id);
        RouteSegmentInfo GetRouteSegmentInfo(Guid id);
        IEnumerable<RouteNodeInfo> GetAllRouteNodes();
        IEnumerable<RouteSegmentInfo> GetAllRouteSegments();
        WalkOfInterestInfo GetWalkOfInterestInfo(Guid id);
        void Clean();
    }
}
