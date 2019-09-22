using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ReadModel
{
    public interface IIdentifiedObject
    {
        Guid mRID { get; }
        string Name { get; }
        string Description { get; }
    }
}
