using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Events.Model
{
    public enum RouteSegmentKindEnum
    {
        Unknown = 0,
        Underground = 1,
        Arial = 2,
        Drilling = 3,
        RoadCrossoverDrilling = 4,
        RoadCrossoverDuctBank = 5,
        MicroTrenching = 6,
        Tunnel = 7,
        Indoor = 8
    }
}
