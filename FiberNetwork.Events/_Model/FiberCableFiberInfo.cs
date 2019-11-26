using Asset.Model;
using Core.ReadModel.Network;
using FiberNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FiberNetwork.Events.Model
{
    /// <summary>
    /// A fiber within a fiber cable
    /// </summary>
    public class FiberCableFiberInfo : FiberInfo
    {
        public override LineKindEnum LineKind { get => LineKindEnum.Fiber; }

        public override string ToString()
        {
            string result = "Fiber no " + SequenceNumber;

            return result;;
        }
    }
}
