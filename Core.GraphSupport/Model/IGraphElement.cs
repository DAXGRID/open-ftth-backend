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

        IEnumerable<IGraphElement> UndirectionalDFS<TNode, TEdge>(Predicate<TNode> nodeCriteria = null, Predicate<TEdge> edgePredicate = null, bool includeElementsWhereCriteriaIsFalse = false) where TNode : GraphElement where TEdge : GraphElement;

    }
}
