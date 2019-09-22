using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.DemoDataBuilder
{
    public static class ColorCodeConverter
    {
        public static ConduitColorEnum GetConduitColorFromCode(string code)
        {
            if (code == "CL")
                return ConduitColorEnum.Clear;
            else if (code == "BL")
                return ConduitColorEnum.Blue;
            else if (code == "OR")
                return ConduitColorEnum.Orange;
            else if (code == "GR")
                return ConduitColorEnum.Green;
            else if (code == "BR")
                return ConduitColorEnum.Brown;
            else if (code == "GY")
                return ConduitColorEnum.Grey;
            else if (code == "WH")
                return ConduitColorEnum.White;
            else if (code == "RD")
                return ConduitColorEnum.Red;
            else if (code == "BK")
                return ConduitColorEnum.Black;
            else if (code == "YL")
                return ConduitColorEnum.Yellow;
            else if (code == "VL")
                return ConduitColorEnum.Violet;
            else if (code == "PI")
                return ConduitColorEnum.Pink;
            else if (code == "AQ")
                return ConduitColorEnum.Aqua;
            else
                throw new ArgumentException("Uknown color code: " + code);
        }
    }
}
