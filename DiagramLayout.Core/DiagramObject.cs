using Core.ReadModel;
using System;

namespace DiagramLayout.Core
{
    public class DiagramObject : IIdentifiedObject
    {
        private Guid _mRID;
        private string _name;
        private IIdentifiedObject _identifiedObject;

        public Guid mRID { get { return _mRID; } }

        public string Name { get { return _name; } }

        public string Description { get { return null; } }

        IIdentifiedObject IdentifiedObject { get { return _identifiedObject; } }
    }
}
