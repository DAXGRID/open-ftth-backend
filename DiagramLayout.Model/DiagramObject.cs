using NetTopologySuite.Geometries;
using System;

namespace DiagramLayout.Model
{
    public class DiagramObject : IDiagramObject
    {
        // Diagram object uuid
        private readonly Guid _mRID;
        public Guid MRID => _mRID;

        // Diagram reference
        private readonly Diagram _diagram;
        public Diagram Diagram => _diagram;

        // Geometry
        public Geometry Geometry { get; set; }

        // Optional stuff
        public IdentifiedObjectReference IdentifiedObject { get; set; }
        public string Style { get; set; }
        public string Label { get; set; }
        public double Rotation { get; set; }

        public DiagramObject()
        {
            _mRID = Guid.NewGuid();
            //_diagram = diagram;
        }
    }
}
