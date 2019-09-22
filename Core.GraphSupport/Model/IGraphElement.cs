using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Core.GraphSupport.Model
{
    public interface IGraphElement
    {
        Guid Id { get;}
        [IgnoreDataMember]
        List<IGraphElement> OutgoingElements { get; }
        [IgnoreDataMember]
        List<IGraphElement> IngoingElements { get; }
        [IgnoreDataMember]
        List<IGraphElement> NeighborElements { get; }
    }
}
