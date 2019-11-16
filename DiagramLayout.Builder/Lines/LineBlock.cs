using DiagramLayout.Builder.Drawing;
using DiagramLayout.Builder.Layout;
using DiagramLayout.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiagramLayout.Builder.Lines
{
    public class LineBlock : DiagramObjectContainer
    {
        private LineBlockTypeEnum _type = LineBlockTypeEnum.ConduitJunction;
        private string _label = null;

        private double _blockMargin = 0;
        public double BlockMargin
        {
            get { return _blockMargin; }
            set { _blockMargin = value; }
        }

        private Guid _refId;
        private string _refClass;

        public void SetReference(Guid refId, string refClass)
        {
            this._refId = refId;
            this._refClass = refClass;
        }

        private Size _desiredSize = new Size();
        

        // Desired Size
        public override Size DesiredSize => _desiredSize;

        // Block sides
        private Dictionary<BlockSideEnum, BlockSide> _sides = new Dictionary<BlockSideEnum, BlockSide>();

        // Connections
        private List<LineBlockPortConnection> _portConnections = new List<LineBlockPortConnection>();

        private List<LineBlockTerminalConnection> _terminalConnections = new List<LineBlockTerminalConnection>();

        private double _offsetX = 0;

        private double _offsetY = 0;


        public LineBlock(double offsetX, double offsetY, LineBlockTypeEnum type = LineBlockTypeEnum.ConduitJunction)
        {
            _type = type;
            _offsetX = offsetX;
            _offsetY = offsetY;
        }

        public void AddPort(BlockPort port)
        {
            if (!_sides.ContainsKey(port.Side))
                _sides.Add(port.Side, new BlockSide(port.Side));

            _sides[port.Side].AddPort(port);
        }

        public void SetSideMargin(double sideMargin)
        {
            foreach (var side in _sides)
            {
                side.Value.SideMargin = sideMargin;
            }
        }

        public void AddTerminalConnection(BlockSideEnum fromSide, int fromPortIndex, int fromTerminalIndex, BlockSideEnum toSide, int toPortIndex, int toTerminalIndex, string label = null, string style = null, LineShapeTypeEnum lineShapeType = LineShapeTypeEnum.Line)
        {
            var connection = new LineBlockTerminalConnection();

            connection.Label = label;
            connection.Style = style;
            connection.LineShapeType = lineShapeType;
            connection.FromTerminal = _sides[fromSide].GetPortByIndex(fromPortIndex).GetTerminalByIndex(fromTerminalIndex);
            connection.ToTerminal = _sides[toSide].GetPortByIndex(toPortIndex).GetTerminalByIndex(toTerminalIndex);

            _terminalConnections.Add(connection);
        }

        public LineBlockPortConnection AddPortConnection(BlockSideEnum fromSide, int fromPortIndex, BlockSideEnum toSide, int toPortIndex, string label = null, string style = null)
        {
            var connection = new LineBlockPortConnection();

            connection.Label = label;
            connection.Style = style;
            connection.FromPort = _sides[fromSide].GetPortByIndex(fromPortIndex);
            connection.ToPort = _sides[toSide].GetPortByIndex(toPortIndex);

            _portConnections.Add(connection);

            return connection;
        }

        public override Size Measure(Size availableSize)
        {
            // Calculate width
            double width = 0;

            foreach (var side in _sides.Where(s => s.Key == BlockSideEnum.North || s.Key == BlockSideEnum.South))
            {
                if (side.Value.Length > width)
                    width = side.Value.Length;
            }

            width += (_blockMargin * 2);

            if (width < MinWidth)
                width = MinWidth;

            // Calculate height
            double height = 0;

            foreach (var side in _sides.Where(s => s.Key == BlockSideEnum.Vest || s.Key == BlockSideEnum.East))
            {
                if (side.Value.Length > height)
                    height = side.Value.Length;
            }

            height += (_blockMargin * 2);

            if (height < MinHeight)
                height = MinHeight;

            _desiredSize = new Size() { Width = width, Height = height };

            return _desiredSize;
        }

        public override Size Arrange(Size finalSize)
        {
            return Measure(finalSize);
        }

        public override IEnumerable<DiagramObject> CreateDiagramObjects(double offsetXparam, double offsetYparam)
        {
            if (_offsetX == 0)
            {

            }

            List<DiagramObject> result = new List<DiagramObject>();

            // Create rect to show where block is
            if (_type != LineBlockTypeEnum.Simple)
            {
                result.Add(new DiagramObject
                {
                    Style = "Well",
                    Geometry = GeometryBuilder.Rectangle(_offsetX, _offsetY, DesiredSize.Height, DesiredSize.Width),
                    IdentifiedObject = _refClass == null ? null : new IdentifiedObjectReference() { RefId = _refId, RefClass = _refClass }
                });
            }

            // Add all side objects
            foreach (var side in _sides.Values)
            {
                result.AddRange(side.CreateDiagramObjects(CalculateSideXOffset(side.Side, _offsetX), CalculateSideYOffset(side.Side, _offsetY), _type));
            }

            // Create all connections

            foreach (var connection in _portConnections)
            {
                result.AddRange(connection.CreateDiagramObjects());
            }


            foreach (var connection in _terminalConnections)
            {
                result.AddRange(connection.CreateDiagramObjects());
            }
            
            return result;
        }

        private double CalculateSideXOffset(BlockSideEnum side, double offsetX)
        {
            if (side == BlockSideEnum.Vest)
                return offsetX; 
            else if (side == BlockSideEnum.North)
                return offsetX + _blockMargin;
            else if (side == BlockSideEnum.East)
                return offsetX + DesiredSize.Width;
            else if (side == BlockSideEnum.South)
                return offsetX + _blockMargin;
            else
                return 0;
        }

        private double CalculateSideYOffset(BlockSideEnum side, double offsetY)
        {
            if (side == BlockSideEnum.Vest)
                return offsetY + _blockMargin;
            else if (side == BlockSideEnum.North)
                return offsetY + DesiredSize.Height;
            else if (side == BlockSideEnum.East)
                return offsetY + _blockMargin;
            else if (side == BlockSideEnum.South)
                return offsetY;
            else
                return 0;
        }

        public void SetSideMargin(BlockSideEnum side, int margin)
        {
            _sides[side].SideMargin = margin;
        }
    }
}
