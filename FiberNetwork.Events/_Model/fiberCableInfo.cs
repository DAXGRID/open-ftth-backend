using Asset.Model;
using FiberNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FiberNetwork.Events.Model
{
    public class FiberCableInfo : FiberInfo
    {
        public override string ToString()
        {
            string result = "Nexans " + Children.Count + " fiber";

            return result;;
        }
    }
}
