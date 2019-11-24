using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel.ConduitClosure;
using DiagramLayout.Builder.Lines;
using DiagramLayout.Builder.Mockup;
using DiagramLayout.Model;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiagramLayout.Builder
{
    public class ConduitClosureBuilder
    {
        private IConduitNetworkQueryService _conduitNetworkQueryService;

        public Diagram Build(Guid nodeId, IRouteNetworkState routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IConduitClosureRepository conduitClosureRepository)
        {
            _conduitNetworkQueryService = conduitNetworkEqueryService;

            DiagramBuilder builder = new DiagramBuilder();

            double minWidth = 300;

            double offsetY = 0;

            ConduitClosureInfo conduitClosureInfo = null;

            if (conduitClosureRepository.CheckIfRouteNodeContainsConduitClosure(nodeId))
            {
                conduitClosureInfo = conduitClosureRepository.GetConduitClosureInfoByRouteNodeId(nodeId);
            }


            // Add cables passing through
            offsetY += AddCablePassThroughBlock(builder, offsetY, 300);


            // Add multi conduit passing through
            var conduitSegmentRels = conduitNetworkEqueryService.GetConduitSegmentsRelatedToPointOfInterest(nodeId);

            foreach (var conduitSegmentRel in conduitSegmentRels)
            {
                // pass by multi conduit
                if (conduitSegmentRel.Type == ConduitNetwork.ReadModel.ConduitRelationTypeEnum.PassThrough && conduitSegmentRel.Segment.Conduit.Kind == ConduitNetwork.Events.Model.ConduitKindEnum.MultiConduit)
                {
                    // check if outside conduit closure
                    if (conduitClosureInfo != null && conduitClosureInfo.Sides.Exists(s => s.Ports.Exists(p => p.MultiConduitId == conduitSegmentRel.Segment.ConduitId)))
                    {
                    }
                    else
                    {
                        offsetY += AddMultiConduitPassThroughBlock(builder, conduitSegmentRel.Segment, minWidth, offsetY);
                    }
                }
            }

            // Add conduit closure
            offsetY += 20;

            if (conduitClosureInfo != null)
            {
                offsetY += AddConduitClosureBlock(builder, conduitClosureInfo, minWidth, offsetY);
            }


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

        public double AddConduitClosureBlock(DiagramBuilder builder, ConduitClosureInfo conduitClosureInfo, double minBlockWidth, double offsetY)
        {
            double labelSectionWidth = 40;
            double sideMargin = 20;

            LineBlock leftLabelBlock = new LineBlock(0, offsetY, LineBlockTypeEnum.Simple);
            leftLabelBlock.MinWidth = labelSectionWidth;

            LineBlock rightLabelBlock = new LineBlock(minBlockWidth + labelSectionWidth, offsetY, LineBlockTypeEnum.Simple);
            rightLabelBlock.MinWidth = labelSectionWidth;


            //////////////////////////////////////////////////////////
            /// conduit closure block
            /// 
            LineBlock conduitClosureBlock = new LineBlock(labelSectionWidth, offsetY);
            conduitClosureBlock.SetReference(conduitClosureInfo.Id, "ConduitClosure");

            conduitClosureBlock.MinWidth = minBlockWidth;
            conduitClosureBlock.MinHeight = 100;

            conduitClosureBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(conduitClosureBlock);

            Dictionary<BlockPort, ConduitClosurePortInfo> blockPortToConduitClosurePort = new Dictionary<BlockPort, ConduitClosurePortInfo>();

            // Add ports
            foreach (var side in conduitClosureInfo.Sides)
            {
                foreach (var conduitClosurePort in side.Ports)
                {
                    var nTerminals = conduitClosurePort.Terminals.Count;

                    var blockPort = AddMultiConduitPort(conduitClosureBlock, Convert(side.Position), conduitClosurePort.Terminals, conduitClosurePort.MultiConduitSegment.Conduit.Color.ToString(), -1, -1, 10);
                    blockPort.SetReference(conduitClosurePort.MultiConduitSegment.Id, "MultiConduitSegment");

                    blockPortToConduitClosurePort.Add(blockPort, conduitClosurePort);

                    // Add left label blocks
                    if (side.Position == ConduitClosureInfoSide.Left)
                    {
                        // Add left west label port
                        AddBigConduitPort(leftLabelBlock, BlockSideEnum.Vest, nTerminals, null, -1, -1, 10);

                        // Add left east label port
                        AddBigConduitPort(leftLabelBlock, BlockSideEnum.East, nTerminals, null, -1, -1, 10);

                        foreach (var terminal in conduitClosurePort.Terminals)
                        {
                            var lineInfo = _conduitNetworkQueryService.CreateConduitLineInfoFromConduitSegment((ConduitSegmentInfo)terminal.LineSegment);
                            leftLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, blockPort.Index, terminal.Position, BlockSideEnum.East, blockPort.Index, terminal.Position, lineInfo.StartRouteNode.Name, "LabelMediumText");
                        }
                    }
                    // Add right label block
                    if (side.Position == ConduitClosureInfoSide.Right)
                    {
                        // Add right west label port
                        AddBigConduitPort(rightLabelBlock, BlockSideEnum.Vest, nTerminals, null, -1, -1, 10);

                        // Add right east label port
                        AddBigConduitPort(rightLabelBlock, BlockSideEnum.East, nTerminals, null, -1, -1, 10);

                        foreach (var terminal in conduitClosurePort.Terminals)
                        {
                            var lineInfo = _conduitNetworkQueryService.CreateConduitLineInfoFromConduitSegment((ConduitSegmentInfo)terminal.LineSegment);
                            rightLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, blockPort.Index, terminal.Position, BlockSideEnum.East, blockPort.Index, terminal.Position, lineInfo.EndRouteNode.Name, "LabelMediumText");
                        }
                    }
                }
            }

            conduitClosureBlock.SetSideCenterAllignment(BlockSideEnum.North, true);

            // Connect ports
            foreach (var portEntry in blockPortToConduitClosurePort)
            {
                var blockPort = portEntry.Key;
                var conduitClosurePort = portEntry.Value;

                if ((blockPort.Side == BlockSideEnum.Vest || blockPort.Side == BlockSideEnum.North) && conduitClosurePort.ConnectionKind == ConduitClosureInternalConnectionKindEnum.PassThrough)
                {
                    var portConnection = conduitClosureBlock.AddPortConnection(blockPort.Side, blockPort.Index, Convert(conduitClosurePort.ConnectedToSide), conduitClosurePort.ConnectedToPort, null, "MultiConduit" + conduitClosurePort.MultiConduitSegment.Conduit.Color.ToString());
                    portConnection.SetReference(conduitClosurePort.MultiConduitSegment.ConduitId, "Conduit");
                }
            }

            // Connect terminals

            HashSet<ConduitClosureTerminalInfo> terminalAlreadyProcessed = new HashSet<ConduitClosureTerminalInfo>();

            foreach (var portEntry in blockPortToConduitClosurePort)
            {
                var blockPort = portEntry.Key;
                var conduitClosurePort = portEntry.Value;

                foreach (var terminal in conduitClosurePort.Terminals)
                {
                    if (!terminalAlreadyProcessed.Contains(terminal))
                    {
                        terminalAlreadyProcessed.Add(terminal);

                        if (terminal.ConnectionKind == ConduitClosureInternalConnectionKindEnum.PassThrough || terminal.ConnectionKind == ConduitClosureInternalConnectionKindEnum.Connected)
                        {
                            string color = "Red";

                            if (terminal.LineSegment is ConduitSegmentInfo)
                            {
                                var conduitSegmentInfo = terminal.LineSegment as ConduitSegmentInfo;
                                color = conduitSegmentInfo.Conduit.Color.ToString();
                            }

                            var terminalConnection = conduitClosureBlock.AddTerminalConnection(blockPort.Side, blockPort.Index, terminal.Position, Convert(terminal.ConnectedToSide), terminal.ConnectedToPort, terminal.ConnectedToTerminal, null, "InnerConduit" + color, LineShapeTypeEnum.Polygon);
                            terminalConnection.SetReference(terminal.LineId, "InnerConduit");

                            // make sure we don't connect the other way too
                            var connectedToTerminal = conduitClosureInfo.Sides.Find(s => s.Position == terminal.ConnectedToSide).Ports.Find(p => p.Position == terminal.ConnectedToPort).Terminals.Find(t => t.Position == terminal.ConnectedToTerminal);
                            terminalAlreadyProcessed.Add(connectedToTerminal);
                        }
                    }

                }

            }


            // Add label blocks
            leftLabelBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(leftLabelBlock);

            rightLabelBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(rightLabelBlock);



            return conduitClosureBlock.DesiredSize.Height;
        }

        private BlockSideEnum Convert(ConduitClosureInfoSide conduitClosureSideEnum)
        {
            if (conduitClosureSideEnum == ConduitClosureInfoSide.Left)
                return BlockSideEnum.Vest;
            else if (conduitClosureSideEnum == ConduitClosureInfoSide.Top)
                return BlockSideEnum.North;
            else if (conduitClosureSideEnum == ConduitClosureInfoSide.Right)
                return BlockSideEnum.East;
            else
                return BlockSideEnum.South;
        }


        public double AddMultiConduitPassThroughBlock(DiagramBuilder builder, ConduitSegmentInfo conduitSegmentInfo, double minBlockWidth, double offsetY)
        {
            double labelSectionWidth = 40;
            double sideMargin = 20;

            //////////////////////////////////////////////////////////
            /// conduit block
            /// 
            LineBlock conduitBlock = new LineBlock(labelSectionWidth, offsetY, LineBlockTypeEnum.Simple);
            conduitBlock.MinWidth = minBlockWidth;

            var nTerminals = conduitSegmentInfo.Children.Count;
            var color = conduitSegmentInfo.Conduit.Color.ToString();

            // Add vest ports
            AddMultiConduitPort(conduitBlock, BlockSideEnum.Vest, nTerminals, color, -1, -1, 10);

            // Add east ports
            AddMultiConduitPort(conduitBlock, BlockSideEnum.East, nTerminals, color, -1, -1, 10);

            // Connect ports
            var portConnection = conduitBlock.AddPortConnection(BlockSideEnum.Vest, 1, BlockSideEnum.East, 1, null, "MultiConduit" + color);
            portConnection.SetReference(conduitSegmentInfo.ConduitId, "Conduit");

            // Connect west and east terminals
            foreach (var child in conduitSegmentInfo.Children.OfType<ConduitSegmentInfo>())
            {
                var terminalConnection = conduitBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, child.Conduit.SequenceNumber, BlockSideEnum.East, 1, child.Conduit.SequenceNumber, null, "InnerConduit" + child.Conduit.Color.ToString(), LineShapeTypeEnum.Polygon);
                terminalConnection.SetReference(child.Conduit.Id, "SingleConduit");
            }

            conduitBlock.SetSideMargin(sideMargin);

            conduitBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(conduitBlock);

            //////////////////////////////////////////////////////////
            /// left label block

            LineBlock leftLabelBlock = new LineBlock(0, offsetY, LineBlockTypeEnum.Simple);
            leftLabelBlock.MinWidth = labelSectionWidth;

            // Add vest port
            AddBigConduitPort(leftLabelBlock, BlockSideEnum.Vest, nTerminals, null, -1, -1, 10);

            // Add east port
            AddBigConduitPort(leftLabelBlock, BlockSideEnum.East, nTerminals, null, -1, -1, 10);

            // Connect west and east terminals
            foreach (var child in conduitSegmentInfo.Children.OfType<ConduitSegmentInfo>())
            {
                var lineInfo = _conduitNetworkQueryService.CreateConduitLineInfoFromConduitSegment(conduitSegmentInfo);

                leftLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, child.Conduit.SequenceNumber, BlockSideEnum.East, 1, child.Conduit.SequenceNumber, lineInfo.StartRouteNode.Name, "LabelMediumText");
            }

            leftLabelBlock.SetSideMargin(sideMargin);

            leftLabelBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(leftLabelBlock);


            //////////////////////////////////////////////////////////
            /// right label block

            LineBlock rightLabelBlock = new LineBlock(minBlockWidth + labelSectionWidth, offsetY, LineBlockTypeEnum.Simple);
            rightLabelBlock.MinWidth = labelSectionWidth;

            // Add vest port
            AddBigConduitPort(rightLabelBlock, BlockSideEnum.Vest, nTerminals, null, -1, -1, 10);

            // Add east port
            AddBigConduitPort(rightLabelBlock, BlockSideEnum.East, nTerminals, null, -1, -1, 10);

            // Connect west and east terminals
            foreach (var child in conduitSegmentInfo.Children.OfType<ConduitSegmentInfo>())
            {
                var lineInfo = _conduitNetworkQueryService.CreateConduitLineInfoFromConduitSegment(conduitSegmentInfo);

                var terminalConnection = rightLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, child.Conduit.SequenceNumber, BlockSideEnum.East, 1, child.Conduit.SequenceNumber, lineInfo.EndRouteNode.Name, "LabelMediumText");
            }

            rightLabelBlock.SetSideMargin(sideMargin);

            rightLabelBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(rightLabelBlock);


            return conduitBlock.DesiredSize.Height;
        }

        public double AddCablePassThroughBlock(DiagramBuilder builder, double offsetY, double minBlockWidth)
        {
            double labelSectionWidth = 40;
            double sideMargin = 20;
            double portMargin = 20;

            //////////////////////////////////////////////////////////
            /// label block
            /// 
            LineBlock cableBlock = new LineBlock(labelSectionWidth, offsetY, LineBlockTypeEnum.Simple);
            cableBlock.MinWidth = minBlockWidth;

            // Add vest ports
            AddBigConduitPort(cableBlock, BlockSideEnum.Vest, 2, null, portMargin);

            // Add east ports
            AddBigConduitPort(cableBlock, BlockSideEnum.East, 2, null, portMargin);

            // Connect west and east terminals
            cableBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 1, BlockSideEnum.East, 1, 1, "72", "CableOutsideWell");
            cableBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 2, BlockSideEnum.East, 1, 2, "72", "CableOutsideWell");

            cableBlock.SetSideMargin(sideMargin);
            cableBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(cableBlock);

           

            //////////////////////////////////////////////////////////
            /// left label block

            LineBlock leftLabelBlock = new LineBlock(0, offsetY, LineBlockTypeEnum.Simple);
            leftLabelBlock.MinWidth = labelSectionWidth;

            // Add vest port
            AddBigConduitPort(leftLabelBlock, BlockSideEnum.Vest, 2, null, portMargin);

            // Add east port
            AddBigConduitPort(leftLabelBlock, BlockSideEnum.East, 2, null, portMargin);

            leftLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 1, BlockSideEnum.East, 1, 1, "FP-0101", "CableOutsideWell");
            leftLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 2, BlockSideEnum.East, 1, 2, "FP-0101", "CableOutsideWell");

            leftLabelBlock.SetSideMargin(sideMargin);
            leftLabelBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(leftLabelBlock);


            //////////////////////////////////////////////////////////
            /// right label block

            LineBlock rightLabelBlock = new LineBlock(minBlockWidth + labelSectionWidth, offsetY, LineBlockTypeEnum.Simple);
            rightLabelBlock.MinWidth = labelSectionWidth;

            // Add vest port
            AddBigConduitPort(rightLabelBlock, BlockSideEnum.Vest, 2, null, portMargin);

            // Add east port
            AddBigConduitPort(rightLabelBlock, BlockSideEnum.East, 2, null, portMargin);

            rightLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 1, BlockSideEnum.East, 1, 1, "SP-1010", "CableOutsideWell");
            rightLabelBlock.AddTerminalConnection(BlockSideEnum.Vest, 1, 2, BlockSideEnum.East, 1, 2, "SP-1020", "CableOutsideWell");

            rightLabelBlock.SetSideMargin(sideMargin);
            rightLabelBlock.Measure(new Layout.Size());
            builder.ContentObjects.Add(rightLabelBlock);


            return cableBlock.DesiredSize.Height;
        }

        private BlockPort AddMultiConduitPort(LineBlock lineBlock, BlockSideEnum side, int nTerminals, string outerConduitColor, double spaceBetweenTerminals = -1, double terminalSize = -1, double portMargin = -1)
        {
            BlockPort port = new BlockPort(side, "MultiConduit" + outerConduitColor, null, spaceBetweenTerminals, terminalSize, portMargin);
         
            for (int i = 0; i < nTerminals; i++)
            {
                var newTerminal = new BlockPortTerminal(true, "InnerConduit" + MockupHelper.GetColorStringFromConduitNumber(i + 1));
                port.AddTerminal(newTerminal);
            }

            lineBlock.AddPort(port);

            return port;
        }

        private BlockPort AddMultiConduitPort(LineBlock lineBlock, BlockSideEnum side, List<ConduitClosureTerminalInfo> terminals, string outerConduitColor, double spaceBetweenTerminals = -1, double terminalSize = -1, double portMargin = -1)
        {
            BlockPort port = new BlockPort(side, "MultiConduit" + outerConduitColor, null, spaceBetweenTerminals, terminalSize, portMargin);

            foreach (var terminal in terminals)
            {
                if (terminal.LineSegment is ConduitSegmentInfo)
                {
                    ConduitSegmentInfo conduitSegment = terminal.LineSegment as ConduitSegmentInfo;
                    bool visible = false;

                    if (terminal.ConnectionKind == ConduitClosureInternalConnectionKindEnum.NotConnected)
                        visible = true;

                    var blockTerminal = new BlockPortTerminal(visible, "InnerConduit" + conduitSegment.Conduit.Color.ToString());
                    blockTerminal.SetReference(conduitSegment.Id, "InnerConduitSegment");
                    port.AddTerminal(blockTerminal);
                }
            }

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
