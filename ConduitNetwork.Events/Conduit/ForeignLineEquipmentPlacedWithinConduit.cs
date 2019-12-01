using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Events
{
    /// <summary>
    /// Event that represent that a line (i.e. fiber cable) is placed within a conduit.
    /// The start and end node is used to indirectly tell which conduit segments that the line is related to.
    /// We deliberatly don't tell which conduit segments the cable is related to, because additional segments are inserted when the conduit is chopped up.
    /// The from and to route node is a more stable way to register the relation between a foreign line and underlying conduit segments.
    /// As you can imagine, many of these events are needed to represent how a fiber cable is blow through several many different conduits.
    /// </summary>
    public class ForeignLineEquipmentPlacedWithinConduit
    {
        /// <summary>
        /// The conduit where the foreign line should be placed. Can be multi conduit, inner conduit of a multi conduit, or a single conduit.
        /// </summary>
        public Guid ConduitId { get; set; }

        /// <summary>
        /// The id of the foreign line to be related to the conduit. It will typical be a fiber cable, but in the future it could be other types as well, such as twister pair network cables etc.
        /// </summary>
        public Guid ForeignLineId { get; set; }

        /// <summary>
        /// At which node that the line starts to be inside the conduit.
        /// </summary>
        public Guid FromRouteNodeId { get; set; }

        /// <summary>
        /// At which node that the line ends being inside the conduit.
        /// </summary>
        public Guid ToRouteNodeId { get; set; }
    }
}
