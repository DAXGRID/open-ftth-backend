using DiagramLayout.Model;
using System;

namespace DiagramLayout.Builder.Rack
{
    public class RackBuilder
    {
        private readonly Diagram _diagram;

        public RackBuilder(Diagram diagram)
        {
            _diagram = diagram;
        }

        public RackDiagramObject New19InchRack(double x, double y, int heightInRackUnits, string rackHeadingText1, string rackHeadingText2)
        {
            var rack = new RackDiagramObject(_diagram, heightInRackUnits);

            /*
            if (rackHeadingText1 != null)
                rack.AddHeadingText(TextType.Heading1, rackHeading1);

            if (rackHeadingText2 != null)
                rack.AddHeadingText(TextType.Heading1, rackHeading2);
            */

            return rack;
        }
    }
}
