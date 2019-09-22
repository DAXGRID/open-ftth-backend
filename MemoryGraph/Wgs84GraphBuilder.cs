using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGraph
{
    /// <summary>
    /// Helper to create a graph from node and link data represented by wgs84 coordinates.
    /// If you have no or partial node data, you need to set allowAutomaticNodeCreation to true.
    /// </summary>
    public class Wgs84GraphBuilder
    {
        Graph _graph;
        HashSet<string> _dublicateKeyCheck = new HashSet<string>();
        Dictionary<string, Node> _nodeLookupByCoord = new Dictionary<string, Node>();
        bool _autoNodeCreation = false;

        public Wgs84GraphBuilder(Graph graph, bool allowAutomaticNodeCreation = false)
        {
            _graph = graph;
            _autoNodeCreation = allowAutomaticNodeCreation;
        }

        public void AddNodeToGraph(string id, double x, double y)
        {
            // Dublicate id check
            if (_dublicateKeyCheck.Contains(id))
                throw new NotSupportedException("Non unique id not allowed: " + id);

            _dublicateKeyCheck.Add(id);

            AddNode(id, x, y);
        }

        public void AddLinkToGraph(string id, double startX, double startY, double endX, double endY)
        {
            // Dublicate id check
            if (_dublicateKeyCheck.Contains(id))
                throw new NotSupportedException("Non unique id not allowed: " + id);

            _dublicateKeyCheck.Add(id);


            // Create link
            var link = new Link() { Id = id };
            _graph.Links.Add(link.Id, link);

            // Create or get start node
            var startNode = CreateOrGetNode(Guid.NewGuid().ToString(), startX, startY);

            if (startNode == null)
                throw new NotSupportedException("Error looking up start node (" + startX + " " + startY + ") of link with id: " + id);

            startNode.Links.Add(link);
            link.StartNode = startNode;

            if (!_graph.Nodes.ContainsKey(startNode.Id))
                _graph.Nodes.Add(startNode.Id, startNode);

            // Create or get end node
            var endNode = CreateOrGetNode(Guid.NewGuid().ToString(), endX, endY);

            if (endNode == null)
                throw new NotSupportedException("Error looking up end node (" + endX + " " + endY + ") of link with id: " + id);

            endNode.Links.Add(link);
            link.EndNode = endNode;

            if (!_graph.Nodes.ContainsKey(endNode.Id))
                _graph.Nodes.Add(endNode.Id, endNode);
        }

        private Node CreateOrGetNode(string nodeId, double x, double y)
        {
            string key = Math.Round(x, 6) + ":" + Math.Round(y, 6);

            if (_nodeLookupByCoord.ContainsKey(key))
            {
                return _nodeLookupByCoord[key];
            }
            else
            {
                if (_autoNodeCreation)
                {
                    var node = new Node() { Id = nodeId, IsAutoCreated = true };
                    _nodeLookupByCoord.Add(key, node);
                    return node;
                }
                else
                {
                    return null;
                }
            }
        }

        private Node AddNode(string nodeId, double x, double y)
        {
            string key = Math.Round(x, 6) + ":" + Math.Round(y, 6);

            var node = new Node() { Id = nodeId, IsAutoCreated = false };
            _nodeLookupByCoord.Add(key, node);
            return node;
        }

    }
}
