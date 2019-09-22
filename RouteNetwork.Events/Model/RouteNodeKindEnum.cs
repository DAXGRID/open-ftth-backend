using System;
using System.Collections.Generic;
using System.Text;

namespace RouteNetwork.Events.Model
{
    public enum RouteNodeKindEnum
    {
        Unknown = 0,
        /// <summary>
        /// bla bla
        /// </summary>
        CentralOfficeBig = 1,
        CentralOfficeSmall = 2,
        CabinetBig = 3,
        CabinetSmall = 4,
        ManHole = 5,
        HandHole = 6,
        ConduitClosure = 7,
        ConduitSimpleJunction = 8,
        ConduitEnd = 9,
        SpliceClosure = 10,
        BuildingAccessPoint = 11,
        MultiDwellingUnit = 12,
        SingleDwellingUnit = 13
    }
}
