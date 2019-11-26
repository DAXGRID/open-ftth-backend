using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public enum LineKindEnum
    {
        Unknown = 0,
        Route = 101,
        MultiConduit = 201,
        InnerConduit = 202,
        SingleConduit = 203,
        FiberCable = 301,
        Fiber = 302
    }
}
