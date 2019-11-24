using FiberNetwork.Events.Model;
using System;

namespace FiberNetwork.QueryService
{
    public interface IFiberNetworkQueryService
    {
        bool CheckIfFiberCableIdExists(Guid id);

        FiberCableInfo GetFiberCableInfo(Guid id);
        
        /*
        ConduitInfo GetConduitInfo(Guid id);


        SingleConduitInfo GetSingleConduitInfo(Guid id);

        SingleConduitSegmentJunctionInfo GetSingleConduitSegmentJunctionInfo(Guid id);
        List<ConduitRelationInfo> GetConduitSegmentsRelatedToPointOfInterest(Guid pointOfInterestId, string conduitId = null);
        List<ConduitRelationInfo> GetConduitSegmentsRelatedToRouteSegment(Guid routeSegmentId, string conduitId = null);

        ConduitLineInfo CreateConduitLineInfoFromConduitSegment(ConduitSegmentInfo sourceConduitSegment);
        */

        void Clean();
    }
}
