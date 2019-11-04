using DiagramLayout.Builder.Layout;
using DiagramLayout.Model;
using System.Collections.Generic;

namespace DiagramLayout.Builder
{
    public class DiagramBuilder
    {
        private Size _canvasSize;

        public List<DiagramObjectContainer> ContentObjects = new List<DiagramObjectContainer>();

        public DiagramBuilder()
        {
            _canvasSize = new Size();
        }

        public Diagram CreateDiagram()
        {
            var diagram = new Diagram();

            foreach (var content in ContentObjects)
            {

                content.Measure(_canvasSize);

                diagram.DiagramObjects.AddRange(content.CreateDiagramObjects(0, 0));
            }

            return diagram;
        }
    }
}
