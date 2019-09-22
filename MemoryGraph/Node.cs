using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGraph
{
    public class Node : GraphElement
    {
        public List<Link> Links = new List<Link>();
        public bool IsAutoCreated = false;
    }
}
