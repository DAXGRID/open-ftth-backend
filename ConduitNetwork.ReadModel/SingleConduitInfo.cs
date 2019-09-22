using Asset.Model;
using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ConduitNetwork.ReadModel
{
    public class SingleConduitInfo : ConduitInfo
    {
        public Guid MultiConduitId { get; set; }

        public override string ToString()
        {
            string result = Name;

            if (Position != 0)
                result += " (" + Position + ")";

            if (Parent != null)
                result += " -> " + Parent.ToString();

            return result;
        }

      
    }
}
