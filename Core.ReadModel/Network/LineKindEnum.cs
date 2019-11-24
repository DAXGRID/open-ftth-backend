using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public enum LineKindEnum
    {
        Route = 101,
        Conduit = 201,
        SignalCobberCable = 301,
        SignalFiberCable = 302,
        SignalCoaxCable = 303,
        SignalTwistedPairCable = 304,
        PowerCable = 401,
        PowerOverheadLine = 402,
        WaterMain = 501,
        SewerMain = 601,
        RemoteHeatingMain = 701
    }
}
