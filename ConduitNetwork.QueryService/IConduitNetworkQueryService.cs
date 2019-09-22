using ConduitNetwork.Events.Model;
using ConduitNetwork.ReadModel;
using System;
using System.Collections.Generic;

namespace ConduitNetwork.QueryService
{
    public interface IConduitNetworkQueryService
    {
        bool CheckIfMultiConduitIdExists(Guid id);

        bool CheckIfConduitIsCut(Guid conduitId, Guid pointOfInterestId);

        bool CheckIfSingleConduitIdExists(Guid id);

        ConduitInfo GetConduitInfo(Guid id);

        MultiConduitInfo GetMultiConduitInfo(Guid id);

        SingleConduitInfo GetSingleConduitInfo(Guid id);

        SingleConduitSegmentJunctionInfo GetSingleConduitSegmentJunctionInfo(Guid id);
        List<ConduitRelationInfo> GetConduitSegmentsRelatedToPointOfInterest(Guid pointOfInterestId, string conduitId = null);
        List<ConduitRelationInfo> GetConduitSegmentsRelatedToRouteSegment(Guid routeSegmentId, string conduitId = null);

        ConduitLineInfo CreateConduitLineInfoFromConduitSegment(ConduitSegmentInfo sourceConduitSegment);
        void Clean();
    }
}
