using ConduitNetwork.ReadModel.ConduitClosure;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.QueryService.ConduitClosure
{
    public interface IConduitClosureRepository
    {
        void UpdateConduitClosureInfo(ConduitClosureInfo conduitClosureInfo);
        void RemoveConduitClosureInfo(Guid conduitClosureId);

        ConduitClosureInfo GetConduitClosureInfo(Guid conduitClosureId);
        ConduitClosureInfo GetConduitClosureInfoByRouteNodeId(Guid pointOfInterestId);
        ConduitClosureInfo GetConduitClosureInfoByRelatedLineId(Guid lineId);

        bool CheckIfConduitClosureAlreadyExists(Guid conduitClosureId);
        bool CheckIfRouteNodeContainsConduitClosure(Guid pointOfInterestId);
        bool CheckIfConduitClosureIsRelatedToLine(Guid lineId);

        void Clean();
    }
}
