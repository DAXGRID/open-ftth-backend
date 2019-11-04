using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiagramLayout.Model
{
    public interface IDiagramObject
    {
        Guid MRID { get; }

        Geometry Geometry { get;  }
    }
}
