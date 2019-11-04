using DiagramLayout.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiagramLayout.Builder.Layout
{
    public abstract class DiagramObjectContainer
    {
        public abstract IEnumerable<DiagramObject> CreateDiagramObjects(double offsetX, double offsetY);
        public abstract Size Measure(Size availableSize);
        public abstract Size Arrange(Size finalSize);
        public abstract Size DesiredSize { get; }
        public double Height { get; set;  }
        public double Width { get; set; }
        public double ActualHeight { get; }
        public double ActualWidth { get; }
    }
}
