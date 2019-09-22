using Asset.Model;
using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ConduitNetwork.ReadModel
{
    public class MultiConduitInfo : ConduitInfo
    {
        public override string ToString()
        {
            string result = "";

            if (AssetInfo != null)
            {
                if (AssetInfo.Manufacturer != null && AssetInfo.Manufacturer.Name != null)
                    result += AssetInfo.Manufacturer.Name;

                if (AssetInfo.Model != null && AssetInfo.Model.Name != null)
                    result += " " + AssetInfo.Model.Name;
            }

            result += " " + ColorMarking.ToString();

            return result;;
        }
    }
}
