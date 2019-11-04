using System;
using System.Collections.Generic;
using System.Text;

namespace DiagramLayout.Builder.Layout
{
    public abstract class Panel
    {
        public abstract IEnumerable<DiagramObjectContainer> Children { get;  }
        public abstract void AddChild(DiagramObjectContainer diagramElement);

    }
}
