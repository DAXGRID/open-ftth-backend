using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitNetwork.Business.DemoDataBuilder
{
    public static class Numbers
    {
        private static int _nextConduitNumber = 1;

        public static string GetNextConduitNumber()
        {
            string result = _nextConduitNumber.ToString().PadLeft(6, '0');
            _nextConduitNumber++;

            return result;
        }
    }
}
