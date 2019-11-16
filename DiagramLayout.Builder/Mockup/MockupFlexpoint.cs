using DiagramLayout.Builder.Lines;
using DiagramLayout.Model;

namespace DiagramLayout.Builder.Mockup
{
    public class MockupFlexpoint
    {
        public Diagram Build()
        {
            DiagramBuilder builder = new DiagramBuilder();
            LineBlock wellBlock = new LineBlock(30, 0);

            // Add first vest port with 5 terminal
            AddMultiConduitPort(wellBlock, BlockSideEnum.Vest, 5, "Orange");

            // Add second vest port with 7 terminal
            AddMultiConduitPort(wellBlock, BlockSideEnum.Vest, 7, "Orange");

            // Add north big conduit port 1 with 3 terminal
            AddBigConduitPort(wellBlock, BlockSideEnum.North, 3, "Red");

            // Add north big conduit port 2 with 5 terminal
            AddBigConduitPort(wellBlock, BlockSideEnum.North, 5, "Red");

            // Add east port with 10 terminal
            AddMultiConduitPort(wellBlock, BlockSideEnum.East, 10, "Orange");

            // Feeder calbe from central office
            wellBlock.AddTerminalConnection(BlockSideEnum.Vest, 2, 5, BlockSideEnum.North, 1, 1, "192", "CableInsideWell");

            // Transit feeder cable to other flex points
            wellBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 4, BlockSideEnum.North, 1, 2, "96", "CableInsideWell");
            wellBlock.AddTerminalConnection(BlockSideEnum.East, 1, 10, BlockSideEnum.North, 1, 3, "96", "CableInsideWell");

            // Sp connections
            wellBlock.AddTerminalConnection(BlockSideEnum.East, 1, 1, BlockSideEnum.North, 2, 2, "24", "CableInsideWell");
            wellBlock.AddTerminalConnection(BlockSideEnum.East, 1, 2, BlockSideEnum.North, 2, 3, "48", "CableInsideWell");
            wellBlock.AddTerminalConnection(BlockSideEnum.East, 1, 3, BlockSideEnum.North, 2, 4, "48", "CableInsideWell");
            wellBlock.AddTerminalConnection(BlockSideEnum.East, 1, 4, BlockSideEnum.North, 2, 5, "48", "CableInsideWell");
            wellBlock.AddTerminalConnection(BlockSideEnum.Vest, 2, 2, BlockSideEnum.North, 2, 1, "48", "CableInsideWell");



            builder.ContentObjects.Add(wellBlock);
            wellBlock.Measure(new Layout.Size());

            //////////////////////////////////////////////////////////
            /// well north label block

            LineBlock wellNorthLabelBlock = new LineBlock(30, wellBlock.DesiredSize.Height, LineBlockTypeEnum.Simple);
            wellNorthLabelBlock.MinHeight = 30;

            // Add north port with 3 terminal
            AddBigConduitPort(wellNorthLabelBlock, BlockSideEnum.North, 3);

            // Add north port with 5 terminal
            AddBigConduitPort(wellNorthLabelBlock, BlockSideEnum.North, 5);

            // Add south port with 3 terminal
            AddBigConduitPort(wellNorthLabelBlock, BlockSideEnum.South, 3);

            // Add south port with 5 terminal
            AddBigConduitPort(wellNorthLabelBlock, BlockSideEnum.South, 5);

            wellNorthLabelBlock.AddTerminalConnection(BlockSideEnum.North, 1, 1, BlockSideEnum.South, 1, 1, "GSS 1 (1-16)", "CableOutsideWell");
            wellNorthLabelBlock.AddTerminalConnection(BlockSideEnum.North, 1, 2, BlockSideEnum.South, 1, 2, "GSS 1 (1-8)", "CableOutsideWell");
            wellNorthLabelBlock.AddTerminalConnection(BlockSideEnum.North, 1, 3, BlockSideEnum.South, 1, 3, "GSS 1 (9-16)", "CableOutsideWell");

            wellNorthLabelBlock.AddTerminalConnection(BlockSideEnum.North, 2, 1, BlockSideEnum.South, 2, 1, "GPS 1", "CableOutsideWell");
            wellNorthLabelBlock.AddTerminalConnection(BlockSideEnum.North, 2, 2, BlockSideEnum.South, 2, 2, "GPS 1", "CableOutsideWell");
            wellNorthLabelBlock.AddTerminalConnection(BlockSideEnum.North, 2, 3, BlockSideEnum.South, 2, 3, "GPS 2", "CableOutsideWell");
            wellNorthLabelBlock.AddTerminalConnection(BlockSideEnum.North, 2, 4, BlockSideEnum.South, 2, 4, "GPS 2", "CableOutsideWell");
            wellNorthLabelBlock.AddTerminalConnection(BlockSideEnum.North, 2, 5, BlockSideEnum.South, 2, 5, "GPS 2 & 3", "CableOutsideWell");


            builder.ContentObjects.Add(wellNorthLabelBlock);
            wellNorthLabelBlock.Measure(new Layout.Size());

            //////////////////////////////////////////////////////////
            /// well vest label block

            LineBlock wellVestLabelBlock = new LineBlock(0, 0, LineBlockTypeEnum.Simple);
            wellVestLabelBlock.MinWidth = 30;

            // Add vest port with 5 terminal
            AddBigConduitPort(wellVestLabelBlock, BlockSideEnum.Vest, 5);

            // Add vest port with 7 terminal
            AddBigConduitPort(wellVestLabelBlock, BlockSideEnum.Vest, 7);

            // Add east port with 5 terminal
            AddBigConduitPort(wellVestLabelBlock, BlockSideEnum.East, 5);

            // Add east port with 7 terminal
            AddBigConduitPort(wellVestLabelBlock, BlockSideEnum.East, 7);

            wellVestLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 4, BlockSideEnum.East, 1, 4, "PF-4200", "CableOutsideWell");
            wellVestLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 2, 2, BlockSideEnum.East, 2, 2, "SP-5420", "CableOutsideWell");
            wellVestLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 2, 5, BlockSideEnum.East, 2, 5, "CO-1010", "CableOutsideWell");

            wellVestLabelBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(wellVestLabelBlock);

            //////////////////////////////////////////////////////////
            /// east label block
            
            LineBlock wellEastLabelBlock = new LineBlock(wellBlock.DesiredSize.Width + 30, 0, LineBlockTypeEnum.Simple);
            wellEastLabelBlock.MinWidth = 30;
            wellEastLabelBlock.MinHeight = wellBlock.DesiredSize.Height;

            // Add vest port with 10 terminal
            AddBigConduitPort(wellEastLabelBlock, BlockSideEnum.Vest, 10);

            // Add east port with 10 terminal
            AddBigConduitPort(wellEastLabelBlock, BlockSideEnum.East, 10);
                    
            wellEastLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 1, BlockSideEnum.East, 1, 1, "SP-5010", "CableOutsideWell");
            wellEastLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 2, BlockSideEnum.East, 1, 2, "SP-5011", "CableOutsideWell");
            wellEastLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 3, BlockSideEnum.East, 1, 3, "SP-5013", "CableOutsideWell");
            wellEastLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 4, BlockSideEnum.East, 1, 4, "SP-6002", "CableOutsideWell");
            wellEastLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 10, BlockSideEnum.East, 1, 10, "FP-4203", "CableOutsideWell");

            wellEastLabelBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(wellEastLabelBlock);
            


            //////////////////////////////////////////////////////////
            /// well north corner 1

            LineBlock wellNorthCorner1 = new LineBlock(30, wellBlock.DesiredSize.Height + wellNorthLabelBlock.DesiredSize.Height, LineBlockTypeEnum.Simple);
            wellNorthCorner1.MinHeight = 20;

            // Add south port with 3 terminal
            AddBigConduitPort(wellNorthCorner1, BlockSideEnum.South, 3);

            // Add east port with 3 terminal
            AddBigConduitPort(wellNorthCorner1, BlockSideEnum.East, 3, null, 1, 1);

            wellNorthCorner1.AddTerminalConnection(BlockSideEnum.South, 1, 1, BlockSideEnum.East, 1, 1, "", "CableOutsideWell");
            wellNorthCorner1.AddTerminalConnection(BlockSideEnum.South, 1, 2, BlockSideEnum.East, 1, 2, "", "CableOutsideWell");
            wellNorthCorner1.AddTerminalConnection(BlockSideEnum.South, 1, 3, BlockSideEnum.East, 1, 3, "", "CableOutsideWell");

            // Set margin on east side to 0
            wellNorthCorner1.SetSideMargin(BlockSideEnum.East, 0);

            //builder.ContentObjects.Add(wellNorthCorner1);

            Diagram diagram = builder.CreateDiagram();

            return diagram;
        }

        private BlockPort AddMultiConduitPort(LineBlock lineBlock, BlockSideEnum side, int nTerminals, string outerConduitColor, double spaceBetweenTerminals = -1, double terminalSize = -1)
        {
            BlockPort port = new BlockPort(side, "MultiConduit" + outerConduitColor, null, spaceBetweenTerminals, terminalSize);

            for (int i = 0; i < nTerminals; i++)
                port.AddTerminal(new BlockPortTerminal(true, "InnerConduit" + MockupHelper.GetColorStringFromConduitNumber(i+1)));

            lineBlock.AddPort(port);

            return port;
        }

        private BlockPort AddBigConduitPort(LineBlock lineBlock, BlockSideEnum side, int nTerminals, string outerConduitColor = null, double spaceBetweenTerminals = -1, double terminalSize = -1)
        {
            BlockPort port = new BlockPort(side, outerConduitColor != null ? "BigConduit" + outerConduitColor : null, null, spaceBetweenTerminals, terminalSize);

            for (int i = 0; i < nTerminals; i++)
                port.AddTerminal(new BlockPortTerminal(false));

            lineBlock.AddPort(port);

            return port;
        }
    }
}
