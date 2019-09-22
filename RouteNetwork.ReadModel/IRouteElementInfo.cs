using Core.GraphSupport.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.ReadModel
{
    public interface IRouteElementInfo : IGraphElement
    {
        Guid Id { get; }

        List<WalkOfInterestInfo> WalkOfInterests { get; }
    }
}
