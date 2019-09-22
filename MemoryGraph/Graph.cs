using QuickGraph;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGraph
{
    public class Graph
    {
        public Dictionary<string, Link> Links = new Dictionary<string, Link>();

        public Dictionary<string, Node> Nodes = new Dictionary<string, Node>();

        private UndirectedGraph<string, Edge<string>> _g;

        private Dictionary<object, string> _edgeToLinkId = new Dictionary<object, string>();


        public List<string> ShortestPath(string fromNodeId, string toNodeId)
        {
            List<string> result = new List<string>();
            //Func<Edge<string>, double> lineDistances = e => 1; // constant cost

            Func<Edge<string>, double> lineDistances = e => Links[_edgeToLinkId[e]].Length;

            TryFunc<string, IEnumerable<Edge<string>>> tryGetPath = GetGraphForTracing().ShortestPathsDijkstra(lineDistances, fromNodeId);

            IEnumerable<Edge<string>> path;
            tryGetPath(toNodeId, out path);

            if (path != null)
            {
                foreach (var edge in path)
                {
                    result.Add(_edgeToLinkId[edge]);
                }
            }

            return result;
        }

        public List<string> ShortestPathOnGraphSubset(string fromNodeId, string toNodeId, List<string> nodes)
        {
            List<string> result = new List<string>();

            // For fast node existence check
            HashSet<string> nodeCheck = new HashSet<string>();
            foreach (var nodeId in nodes)
                nodeCheck.Add(nodeId);

            Dictionary<object, string> tempEdgeToLinkId = new Dictionary<object, string>();

            // Create temp graph with 
            var tempGraph = new UndirectedGraph<string, Edge<string>>();

            // Add vertices and edges
            foreach (var nodeId in nodes)
            {
                var node = Nodes[nodeId];

                tempGraph.AddVertex(node.Id);
            }

            foreach (var nodeId in nodes)
            {
                var node = Nodes[nodeId];

                foreach (var link in node.Links)
                {
                    if (nodeCheck.Contains(link.StartNode.Id) && nodeCheck.Contains(link.EndNode.Id))
                    {

                        var edge = new Edge<string>(link.StartNode.Id, link.EndNode.Id);

                        tempGraph.AddEdge(edge);
                        tempEdgeToLinkId.Add(edge, link.Id);
                    }
                }
            }

            //Func<Edge<string>, double> lineDistances = e => 1; // constant cost

            Func<Edge<string>, double> lineDistances = e => Links[tempEdgeToLinkId[e]].Length;

            TryFunc<string, IEnumerable<Edge<string>>> tryGetPath = tempGraph.ShortestPathsDijkstra(lineDistances, fromNodeId);

            IEnumerable<Edge<string>> path;
            tryGetPath(toNodeId, out path);

            if (path != null)
            {
                foreach (var edge in path)
                {
                    result.Add(tempEdgeToLinkId[edge]);
                }
            }

            return result;
        }

        public List<string> FindLinkPathEnds(List<string> links)
        {
            List<string> result = new List<string>();

            foreach (var linkId in links)
            {
                var graphLink = Links[linkId];

                // Check if we find no links (in the links list) related to the start node. If that's the case, it's an end
                bool linkStartFound = true;

                foreach (var startLink in graphLink.StartNode.Links)
                {
                    if (startLink.Id != linkId && links.Exists(id => id == startLink.Id))
                        linkStartFound = false;
                }

                if (linkStartFound)
                {
                    result.Add(linkId);
                }


                // Check if we find no links (in the links list) related to the end node. If that's the case, it's an end
                bool linkEndFound = true;

                foreach (var endLink in graphLink.EndNode.Links)
                {
                    if (endLink.Id != linkId && links.Exists(id => id == endLink.Id))
                        linkEndFound = false;
                }

                if (linkEndFound)
                {
                    result.Add(linkId);
                }
            }

            return result;
        }

        public List<string> SortLinkPath(List<string> links)
        {
            if (links.Count == 1)
                return new List<string>() { links[0] };

            var ends = FindLinkPathEnds(links);

            if (ends.Count < 1)
                throw new NotSupportedException("No ends found in path. Make sure the links represent a path (not a trail or walk with repeating edges or vertices). Links: " + IdStringList(links));

            if (ends.Count == 1)
                throw new NotSupportedException("Only one end found in path. Make sure the links represent a path (not a trail or walk with repeating edges or vertices). Links: " + IdStringList(links));

            if (ends.Count > 2)
                throw new NotSupportedException(ends.Count + " found in path. Make sure the links represent a connected path. Ends: " + IdStringList(ends));

            List<string> linksSorted = new List<string>();
            List<string> linksRemaning = new List<string>();
            linksRemaning.AddRange(links);

            var currentId = ends[0];
            linksRemaning.Remove(currentId);
            
            while (currentId != null)
            {
                linksSorted.Add(currentId);

                var currentLink = Links[currentId];
                currentId = null;

                foreach (var neighborLink in currentLink.NeighborLinks())
                {
                    if (linksRemaning.Contains(neighborLink))
                    {
                        currentId = neighborLink;
                        linksRemaning.Remove(neighborLink);
                    }
                }
            }

            if (linksSorted.Count != links.Count)
                throw new NotSupportedException("Only " + linksSorted.Count + " out of " + links.Count + " could be sorted. Make sure the links represent a connected path. Links:" + IdStringList(links));

            return linksSorted;

        }

        /// <summary>
        /// Returns a list of node-link-node-link-node etc. from a list of link (ids)
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public List<Guid> GetNodeLinkPathFromLinkPath(List<string> links)
        {
            List<Guid> result = new List<Guid>();

            // Do a sort, also to check if link represent a valid path
            var sortedLinks = SortLinkPath(links);

            bool firstLink = true;

            Node prevNode = null;

            foreach (var linkId in sortedLinks)
            {
                var currentLink = Links[linkId];

                if (firstLink)
                {
                    // if more than one link, we need start with the right node
                    if (sortedLinks.Count > 1)
                    {
                        if (!currentLink.StartNode.Links.Contains(Links[sortedLinks[1]]))
                        {
                            result.Add(Guid.Parse(currentLink.StartNode.Id));
                            prevNode = currentLink.StartNode;
                        }
                        else
                        {
                            result.Add(Guid.Parse(currentLink.EndNode.Id));
                            prevNode = currentLink.EndNode;
                        }
                    }
                    else
                    {
                        result.Add(Guid.Parse(currentLink.StartNode.Id));
                        prevNode = currentLink.StartNode;
                    }
                }

                // add the link
                result.Add(Guid.Parse(linkId));

                // add the node
                var nextNode = GetLinkOtherEnd(Links[linkId], prevNode);
                result.Add(Guid.Parse(nextNode.Id));

                prevNode = nextNode;
                firstLink = false;
            }

            return result;
        }

        private Node GetLinkOtherEnd(Link link, Node end)
        {
            if (link.StartNode == end)
                return link.EndNode;
            else
                return link.StartNode;
        }


        private string IdStringList(List<string> ids)
        {
            string idStr = "";
            foreach (var id in ids)
            {
                if (idStr.Length > 1)
                    idStr += ",";

                idStr += id;
            }

            return idStr;
        }


        private UndirectedGraph<string, Edge<string>> GetGraphForTracing()
        {
            if (_g != null)
                return _g;

            _g = new UndirectedGraph<string, Edge<string>>();

            foreach (var node in Nodes)
            {
                _g.AddVertex(node.Key);
            }

            foreach (var link in Links)
            {
                var edge = new Edge<string>(link.Value.StartNode.Id, link.Value.EndNode.Id);
                _g.AddEdge(edge);
                _edgeToLinkId.Add(edge, link.Key);
            }

            return _g;
        }
    }



   

}
