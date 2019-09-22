﻿using Core.GraphSupport.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.GraphSupport.Traversal
{
    public class BasicTraversal<TNode, TEdge> where TNode : GraphElement where TEdge : GraphElement
    {
        private GraphElement _startElement = null;

        public BasicTraversal(GraphElement startElement)
        {
            _startElement = startElement;
        }

        /// <summary>
        /// Conducts a undirected Depth First Search (DFS)
        /// </summary>
        /// <param name="nodeCriteria">
        /// A criteria that is evaluated on nodes visited.
        /// If true, the node will be included in the result, and all edges (connection to other nodes) will be followed.
        /// If false, the node will not be included in the result, and the traversal will not folow any more edges from this node.
        /// </param>
        /// <returns>Both nodes and edged will be returned in the result.</returns>
        public List<IGraphElement> UndirectedDFS(Predicate<TNode> nodeCriteria, Predicate<TEdge> edgeCriteria, bool includeNodesWhereCriteriaIsFalse = false)
        {
            Queue<IGraphElement> traverseOrder = new Queue<IGraphElement>();
            Stack<IGraphElement> stack = new Stack<IGraphElement>();
            HashSet<IGraphElement> visited = new HashSet<IGraphElement>();

            stack.Push(_startElement);
            visited.Add(_startElement);

            while (stack.Count > 0)
            {
                IGraphElement p = stack.Pop();

                traverseOrder.Enqueue(p);

                var neighbors = p.NeighborElements;

                foreach (var neighbor in neighbors)
                {
                    // If we're dealing with an edge
                    if (neighbor is TEdge)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);

                            if (edgeCriteria == null)
                                stack.Push(neighbor);
                            else if (edgeCriteria.Invoke((TEdge)neighbor))
                                stack.Push(neighbor);
                            else
                            {
                                if (includeNodesWhereCriteriaIsFalse)
                                    traverseOrder.Enqueue(neighbor);
                            }
                        }
                    }
                    // We're dealing with a node
                    else
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);

                            if (nodeCriteria == null)
                                stack.Push(neighbor);
                            else if (nodeCriteria.Invoke((TNode)neighbor))
                                stack.Push(neighbor);
                            else
                            {
                                if (includeNodesWhereCriteriaIsFalse)
                                    traverseOrder.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }

            return traverseOrder.ToList();
        }
    }
}
