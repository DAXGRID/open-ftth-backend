using Core.GraphSupport.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel.Network
{
    public interface ILine
    {
        Guid Id { get;}

        Guid WalkOfInterestId { get; }

        LineKindEnum LineKind { get; }

        /// <summary>
        /// The original start of the line equipment placed in the network
        /// </summary>
        INode FromRouteNode { get; }

        /// <summary>
        /// The original end of the line equipment placed in the network
        /// </summary>
        INode ToRouteNode { get; }

        /// <summary>
        /// Parent line, if line is contained by another line
        /// </summary>
        ILine Parent { get; set; }

        /// <summary>
        /// Child lines, if line contains other lines
        /// </summary>
        List<ILine> Children { get; set; }

        /// <summary>
        /// The line segments (occuring when chopped it up)
        /// </summary>
        List<ISegment> Segments { get; set; }

        /// <summary>
        /// Position within parent line
        /// </summary>
        int SequenceNumber { get; set; }

        ILine GetRoot();
    }
}
