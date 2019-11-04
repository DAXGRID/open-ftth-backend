using System;
using System.Collections.Generic;
using System.Text;

namespace DiagramLayout.Builder.Layout
{
    public class StackPanel : Panel
    {
        public override IEnumerable<DiagramObjectContainer> Children => throw new NotImplementedException();

        public override void AddChild(DiagramObjectContainer diagramElement)
        {
            throw new NotImplementedException();
        }
    }
}
