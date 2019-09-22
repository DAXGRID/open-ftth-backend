using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Events.Model
{
    public enum RouteNodeFunctionKindEnum
    {
        Unknown = 0,
        PrimaryNode = 1,
        SecondaryNode = 2,
        FlexPoint = 3,
        SplicePoint = 4,
        ServiceDeliveryPoint = 5,
        OpenConduitPoint = 6,
        BurriedConduitPont = 7
    }
}
