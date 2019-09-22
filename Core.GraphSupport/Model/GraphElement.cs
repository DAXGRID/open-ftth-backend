using Core.GraphSupport.Traversal;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Core.GraphSupport.Model
{
    public abstract class GraphElement : IGraphElement
    {
        public Guid Id { get; set; }
        [IgnoreDataMember]
        public abstract List<IGraphElement> OutgoingElements { get; }
        [IgnoreDataMember]
        public abstract List<IGraphElement> IngoingElements { get; }
        [IgnoreDataMember]
        public abstract List<IGraphElement> NeighborElements { get; }

        public IEnumerable<IGraphElement> UndirectionalDFS<TNode, TEdge>(Predicate<TNode> nodeCriteria = null, Predicate<TEdge> edgePredicate = null, bool includeElementsWhereCriteriaIsFalse = false) where TNode : GraphElement where TEdge : GraphElement
        {
            var traversal = new BasicTraversal<TNode, TEdge>(this);

            return traversal.UndirectedDFS(nodeCriteria, edgePredicate, includeElementsWhereCriteriaIsFalse);
        }

        public IEnumerable<IGraphElement> ShortestPath(IGraphElement toNode)
        {
            return null;
        }
    }
}
