using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGraph
{
    public class Link : GraphElement
    {
        public Node StartNode { get; set; }
        public Node EndNode { get; set; }
        public double Length { get; set; }

        public List<string> NeighborLinks()
        {
            List<string> result = new List<string>();

            result.AddRange(StartNode.Links.Where(l => l != this).Select(l => l.Id));
            result.AddRange(EndNode.Links.Where(l => l != this).Select(l => l.Id));

            return result;
        }
    }
}
