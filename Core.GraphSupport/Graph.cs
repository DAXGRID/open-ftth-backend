using Core.GraphSupport.Model;
using QuickGraph;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentService
{
    public class Graph
    {
        private UndirectedGraph<string, Edge<string>> _g = new UndirectedGraph<string, Edge<string>>();

        Dictionary<Edge<string>, IGraphElement> _edgeIndex = new Dictionary<Edge<string>, IGraphElement>();
        Dictionary<string, IGraphElement> _nodeIndex = new Dictionary<string, IGraphElement>();

        public Graph(List<IGraphElement> graphElements)
        {
            // treat all conducting equipments as nodes - neighboor relations as links

            var nodes = graphElements.FindAll(o => o is GraphNode);

            HashSet<string> ciCheck = new HashSet<string>();

            foreach (var ci in nodes)
            {
                ciCheck.Add(ci.Id.ToString());
                _g.AddVertex(ci.Id.ToString());

                _nodeIndex.Add(ci.Id.ToString(), ci);
            }

            var edges = graphElements.FindAll(o => o is GraphEdge);

            foreach (var edge in edges)
            {
                var graphEdge = new Edge<string>(edge.NeighborElements[0].Id.ToString(), edge.NeighborElements[1].Id.ToString());
                _g.AddEdge(graphEdge);
                _edgeIndex.Add(graphEdge, edge);
            }

        }

        public List<IGraphElement> ShortestPath(string fromNodeId, string toNodeId)
        {
            List<IGraphElement> result = new List<IGraphElement>();
            Func<Edge<string>, double> distances = e => 1; // constant cost

            TryFunc<string, IEnumerable<Edge<string>>> tryGetPath = _g.ShortestPathsDijkstra(distances, fromNodeId);

            IEnumerable<Edge<string>> path;
            tryGetPath(toNodeId, out path);

            if (path != null)
            {
                var nextExpectedNode = Guid.Parse(fromNodeId);

                foreach (var edge in path)
                {
                    var graphEdge = _edgeIndex[edge];
                    var graphFromNode = _nodeIndex[edge.Source];
                    var graphToNode = _nodeIndex[edge.Target];

                    if (graphFromNode.Id == nextExpectedNode)
                    {
                        // add expected node
                        if (!result.Contains(graphFromNode))
                            result.Add(graphFromNode);

                        // add edge
                        if (!result.Contains(graphEdge))
                            result.Add(graphEdge);

                        // add other end
                        if (!result.Contains(graphToNode))
                            result.Add(graphToNode);

                        nextExpectedNode = graphToNode.Id;
                    }
                    else
                    {
                        // add expected node
                        if (!result.Contains(graphToNode))
                            result.Add(graphToNode);

                        // add edge
                        if (!result.Contains(graphEdge))
                            result.Add(graphEdge);

                        // add other end
                        if (!result.Contains(graphFromNode))
                            result.Add(graphFromNode);

                        nextExpectedNode = graphFromNode.Id;
                    }
                }
            }

            return result;
        }



    }
}
