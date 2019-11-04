using System;
using System.Collections.Generic;
using System.Text;

namespace DiagramLayout.Builder.Mockup
{
    public static class MockupHelper
    {
        public static string GetColorStringFromConduitNumber(int conduitNumber)
        {
            string[] colors = new string[] { "Blue", "Orange", "Green", "Brown", "Grey", "White", "Red", "Black", "Yellow", "Violet", "Pink", "Aqua" };

            return colors[conduitNumber - 1];
        }
    }
}
