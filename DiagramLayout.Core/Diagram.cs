using System;
using System.Collections.Generic;
using System.Text;

namespace DiagramLayout.Core
{
    public class Diagram
    {
        private List<DiagramObject> _diagramObjects = new List<DiagramObject>();

        public IEnumerable<DiagramObject> DiagramObjects
        {
            get
            {
                return _diagramObjects;
            }
       }
    }
}
