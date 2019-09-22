using Location.Model;
using RouteNetwork.Events.Model;
using System;

namespace RouteNetwork.Events
{
    public class RouteNodeAdded
    {
        public Guid RouteNodeId { get; set; }

        // MUST DIE! Should be taken from equipment that is put inside node
        public string Name { get; set; }

        public RouteNodeKindEnum NodeKind { get; set; }

        public RouteNodeFunctionKindEnum NodeFunctionKind { get; set; }

        public Geometry InitialGeometry { get; set; }

        public LocationInfo LocationInfo { get; set; }
    }
}
