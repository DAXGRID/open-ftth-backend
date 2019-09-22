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
        ConduitClosureInfo GetConduitClosureInfoByPointOfInterestId(Guid pointOfInterestId);

        bool CheckIfConduitClosureAlreadyExists(Guid conduitClosureId);
        bool CheckIfConduitClosureAlreadyAddedToPointOfInterest(Guid pointOfInterestId);

        void Clean();
    }
}
