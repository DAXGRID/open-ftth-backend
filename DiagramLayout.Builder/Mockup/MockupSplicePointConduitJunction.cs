using DiagramLayout.Builder.Lines;
using DiagramLayout.Model;

namespace DiagramLayout.Builder.Mockup
{
    public class MockupSplicePointConduitJunction
    {
        public Diagram Build()
        {
            DiagramBuilder builder = new DiagramBuilder();

            double offsetY = 0;

            offsetY += AddCablePassThroughBlock(builder, offsetY, 300);

            AddMultiConduitPassThroughBlock(builder, 300, offsetY, 10);

            Diagram diagram = builder.CreateDiagram();

            return diagram;


            LineBlock junctionBlock = new LineBlock(30, 0);
            junctionBlock.MinWidth = 300;

            // Add first vest port with 5 terminal
            AddMultiConduitPort(junctionBlock, BlockSideEnum.Vest, 10, "Orange");

            // Add second vest port with 7 terminal
            AddMultiConduitPort(junctionBlock, BlockSideEnum.Vest, 7, "Orange");


            // Add fist east port with 10 terminal
            AddMultiConduitPort(junctionBlock, BlockSideEnum.East, 10, "Orange");

            // Add second east port with 7 terminal
            AddMultiConduitPort(junctionBlock, BlockSideEnum.East, 7, "Orange");


            // Add north big conduit port 1 with 3 terminal
            AddBigConduitPort(junctionBlock, BlockSideEnum.North, 3, "Red");

            // Add north big conduit port 2 with 5 terminal
            AddBigConduitPort(junctionBlock, BlockSideEnum.North, 5, "Red");

            junctionBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 1, BlockSideEnum.East, 1, 1, null, "InnerConduitBlue", LineShapeTypeEnum.Polygon);

            junctionBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 2, BlockSideEnum.North, 1, 1, null, "InnerConduitBlue", LineShapeTypeEnum.Polygon);

            /*
         
            // Feeder calbe from central office
            junctionBlock.AddConnection(BlockSideEnum.Vest, 2, 5, BlockSideEnum.North, 1, 1, "192", "CableInsideWell");

            // Transit feeder cable to other flex points
            junctionBlock.AddConnection(BlockSideEnum.Vest, 1, 4, BlockSideEnum.North, 1, 2, "96", "CableInsideWell");
            junctionBlock.AddConnection(BlockSideEnum.East, 1, 10, BlockSideEnum.North, 1, 3, "96", "CableInsideWell");

            // Sp connections
            junctionBlock.AddConnection(BlockSideEnum.East, 1, 1, BlockSideEnum.North, 2, 2, "24", "CableInsideWell");
            junctionBlock.AddConnection(BlockSideEnum.East, 1, 2, BlockSideEnum.North, 2, 3, "48", "CableInsideWell");
            junctionBlock.AddConnection(BlockSideEnum.East, 1, 3, BlockSideEnum.North, 2, 4, "48", "CableInsideWell");
            junctionBlock.AddConnection(BlockSideEnum.East, 1, 4, BlockSideEnum.North, 2, 5, "48", "CableInsideWell");
            junctionBlock.AddConnection(BlockSideEnum.Vest, 2, 2, BlockSideEnum.North, 2, 1, "48", "CableInsideWell");

            */

            builder.ContentObjects.Add(junctionBlock);
            junctionBlock.Measure(new Layout.Size());

            //////////////////////////////////////////////////////////
            /// well north label block

            LineBlock wellNorthLabelBlock = new LineBlock(30, junctionBlock.DesiredSize.Height, LineBlockTypeEnum.Simple);
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
            
            LineBlock wellEastLabelBlock = new LineBlock(junctionBlock.DesiredSize.Width + 30, 0, LineBlockTypeEnum.Simple);
            wellEastLabelBlock.MinWidth = 30;
            wellEastLabelBlock.MinHeight = junctionBlock.DesiredSize.Height;

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

            LineBlock wellNorthCorner1 = new LineBlock(30, junctionBlock.DesiredSize.Height + wellNorthLabelBlock.DesiredSize.Height, LineBlockTypeEnum.Simple);
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

            Diagram sdiagram = builder.CreateDiagram();

            return diagram;
        }

        public double AddMultiConduitPassThroughBlock(DiagramBuilder builder, double minWith, double offsetY, int nTerminals)
        {

            //////////////////////////////////////////////////////////
            /// label block
            /// 
            LineBlock conduitBlock = new LineBlock(30, offsetY, LineBlockTypeEnum.Simple);
            conduitBlock.MinWidth = minWith;

            // Add vest ports
            AddMultiConduitPort(conduitBlock, BlockSideEnum.Vest, nTerminals, "Orange", -1, -1, 10);

            // Add east ports
            AddMultiConduitPort(conduitBlock, BlockSideEnum.East, nTerminals, "Orange", -1, -1, 10);

            // Connect ports
            conduitBlock.AddPortConnection(BlockSideEnum.Vest, 1, BlockSideEnum.East, 1, null, "MultiConduitOrange");

            // Connect west and east terminals
            for (int i = 0; i < nTerminals; i++)
            {
                conduitBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, i + 1, BlockSideEnum.East, 1, i + 1, null, "InnerConduit" + MockupHelper.GetColorStringFromConduitNumber(i + 1), LineShapeTypeEnum.Polygon);
            }

            conduitBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(conduitBlock);

            //////////////////////////////////////////////////////////
            /// label block

            LineBlock labelBlock = new LineBlock(0, offsetY, LineBlockTypeEnum.Simple);
            labelBlock.MinWidth = 30;

            // Add vest port
            AddBigConduitPort(labelBlock, BlockSideEnum.Vest, nTerminals, null, -1, -1, 10);

            // Add east port
            AddBigConduitPort(labelBlock, BlockSideEnum.East, nTerminals, null, -1, -1, 10);

            labelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 1, BlockSideEnum.East, 1, 1, "PF-4200", "LabelMediumText");
       
            labelBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(labelBlock);

            return conduitBlock.DesiredSize.Height;
        }

        public double AddCablePassThroughBlock(DiagramBuilder builder, double offsetY, double minWith)
        {

            //////////////////////////////////////////////////////////
            /// label block
            /// 
            LineBlock cableBlock = new LineBlock(30, offsetY, LineBlockTypeEnum.Simple);
            cableBlock.MinWidth = minWith;

            // Add vest ports
            AddBigConduitPort(cableBlock, BlockSideEnum.Vest, 2, null, 20);

            // Add east ports
            AddBigConduitPort(cableBlock, BlockSideEnum.East, 2, null, 20);

            // Connect west and east terminals
            cableBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 1, BlockSideEnum.East, 1, 1, "72", "CableOutsideWell");
            cableBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 2, BlockSideEnum.East, 1, 2, "72", "CableOutsideWell");

            cableBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(cableBlock);

            //////////////////////////////////////////////////////////
            /// label block

            LineBlock labelBlock = new LineBlock(0, offsetY, LineBlockTypeEnum.Simple);
            labelBlock.MinWidth = 30;

            // Add vest port
            AddBigConduitPort(labelBlock, BlockSideEnum.Vest, 2);

            // Add east port
            AddBigConduitPort(labelBlock, BlockSideEnum.East, 2);

            labelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 1, BlockSideEnum.East, 1, 1, "FP-0101", "CableOutsideWell");
            labelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 1, BlockSideEnum.East, 1, 1, "FP-0101", "CableOutsideWell");

            labelBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(labelBlock);

            return cableBlock.DesiredSize.Height;
        }


        private BlockPort AddMultiConduitPort(LineBlock lineBlock, BlockSideEnum side, int nTerminals, string outerConduitColor, double spaceBetweenTerminals = -1, double terminalSize = -1, double portMargin = -1)
        {
            BlockPort port = new BlockPort(side, "MultiConduit" + outerConduitColor, null, spaceBetweenTerminals, terminalSize, portMargin);

            for (int i = 0; i < nTerminals; i++)
                port.AddTerminal(new BlockPortTerminal(true, "InnerConduit" + MockupHelper.GetColorStringFromConduitNumber(i+1)));

            lineBlock.AddPort(port);

            return port;
        }

        private BlockPort AddBigConduitPort(LineBlock lineBlock, BlockSideEnum side, int nTerminals, string outerConduitColor = null, double spaceBetweenTerminals = -1, double terminalSize = -1, double portMargin = -1)
        {
            BlockPort port = new BlockPort(side, outerConduitColor != null ? "BigConduit" + outerConduitColor : null, null, spaceBetweenTerminals, terminalSize, portMargin);

            for (int i = 0; i < nTerminals; i++)
                port.AddTerminal(new BlockPortTerminal(false));

            lineBlock.AddPort(port);

            return port;
        }
    }
}
