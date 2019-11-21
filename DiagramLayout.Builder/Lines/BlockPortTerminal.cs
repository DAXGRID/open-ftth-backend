using DiagramLayout.Builder.Drawing;
using DiagramLayout.Model;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiagramLayout.Builder.Lines
{
    public class BlockPortTerminal
    {
        private bool _visible = true;
        private string _style = null;
        private string _label = null;

        public BlockPortTerminal(bool visible = true, string style = null, string label = null)
        {
            _visible = visible;
            _style = style;
            _label = label;
        }

        public BlockPort Port { get; set; }

        public Point LineConnectionPoint { get; set; }

        public int Index { get; set; }

        public double ConnectionPointX = 0;
        public double ConnectionPointY = 0;

        private double _length = 8;
        public double Length {
            get { return _length; }
            set { _length = value; }
        }

        private Guid _refId;
        private string _refClass;

        public void SetReference(Guid refId, string refClass)
        {
            this._refId = refId;
            this._refClass = refClass;
        }

        public double Thickness
        {
            get { return Port.PortThickness / 2 + (Port.PortThickness / 2); }
        }

        public List<DiagramObject> CreateDiagramObjects(double offsetX, double offsetY, LineBlockTypeEnum blockType)
        {

            List<DiagramObject> result = new List<DiagramObject>();

            var terminalOffsetX = offsetX;
            var terminalOffsetY = offsetY;

            ConnectionPointX = 0;
            ConnectionPointY = 0;

            // Create terminal diagram object
            var terminalPolygon = new DiagramObject();

            if (_refClass != null)
                terminalPolygon.IdentifiedObject = new IdentifiedObjectReference() { RefId = _refId, RefClass = _refClass };

            terminalPolygon.Style = _style == null ? "LinkBlockTerminal" : _style;

            var rectWidth = Port.IsVertical ? Port.PortThickness + (Port.PortThickness / 2) : Length;
            var rectHeight = Port.IsVertical ? Length : Port.PortThickness + (Port.PortThickness / 2);

            if (Port.Side == BlockSideEnum.Vest)
            {
                //terminalOffsetX += (Port.PortThickness / 2);

                ConnectionPointX = offsetX;
                ConnectionPointY = offsetY + (Length / 2);
            }
            else if (Port.Side == BlockSideEnum.East)
            {
                terminalOffsetX -= (Port.PortThickness + (Port.PortThickness / 2));

                ConnectionPointX = offsetX;
                ConnectionPointY = offsetY + (Length / 2);
            }
            else if (Port.Side == BlockSideEnum.South)
            {
                terminalOffsetX -= Length;
                terminalOffsetY += (Port.PortThickness / 2);

                ConnectionPointX = offsetX + (Length / 2);
                ConnectionPointY = offsetY;
            }
            else if (Port.Side == BlockSideEnum.North)
            {
                terminalOffsetY -= (Port.PortThickness + (Port.PortThickness / 2));  // We need to start on lover y, because we're in the top

                ConnectionPointX = offsetX + (Length / 2);
                ConnectionPointY = offsetY;
            }

            if (_visible && blockType != LineBlockTypeEnum.Simple)
            {
                terminalPolygon.Geometry = GeometryBuilder.Rectangle(terminalOffsetX, terminalOffsetY, rectHeight, rectWidth);

                result.Add(terminalPolygon);

                result.Add(
                    new DiagramObject()
                    {
                        Style = "LinkBlockTerminalConnectionPoint",
                        Geometry = GeometryBuilder.Point(ConnectionPointX, ConnectionPointY)
                    }
                );
            }

            return result;
        }
    }
}
