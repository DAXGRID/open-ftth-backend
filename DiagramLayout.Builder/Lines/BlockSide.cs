using DiagramLayout.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiagramLayout.Builder.Lines
{
    public class BlockSide
    {
        private readonly BlockSideEnum _side;
        public BlockSideEnum Side => _side;

        private double _sideMargin = 40;
        public double SideMargin
        {
            get { return _sideMargin; }
            set { _sideMargin = value; }
        }

        private double _spaceBetweenPorts = 20;

        private List<BlockPort> _ports = new List<BlockPort>();
               
        public BlockSide(BlockSideEnum side)
        {
            _side = side;
        }

        public void AddPort(BlockPort port)
        {
            _ports.Add(port);
            port.Index = _ports.Count;
        }

        public BlockPort GetPortByIndex(int index)
        {
            return _ports.Find(p => p.Index == index);
        }

        public double Length
        {
            get
            {
                double length = 0;

                // Sum port lengths
                foreach (var port in _ports)
                    length += port.Length;

                // Add port spaces to length
                if (_ports.Count > 0)
                    length += (_ports.Count - 1) * _spaceBetweenPorts;

                return length + (_sideMargin * 2);
            }
        }

        public List<DiagramObject> CreateDiagramObjects(double offsetX, double offsetY, LineBlockTypeEnum blockType)
        {
            if (Side == BlockSideEnum.South || Side == BlockSideEnum.East)
                _ports.Reverse();

            List<DiagramObject> result = new List<DiagramObject>();

            double portX = offsetX;
            double portY = offsetY;

            if (_side == BlockSideEnum.Vest)
                portY += _sideMargin;
            else if (_side == BlockSideEnum.North)
                portX += _sideMargin;
            else if (_side == BlockSideEnum.East)
                portY -= _sideMargin;
            else if (_side == BlockSideEnum.South)
                portX -= _sideMargin;


            foreach (var port in _ports)
            {

                double xStep = 1;
                double yStep = 1;

                if (_side == BlockSideEnum.Vest)
                {
                    // goes up y
                    xStep = 0;
                    yStep = port.Length + _spaceBetweenPorts;
                }

                if (_side == BlockSideEnum.North)
                {
                    // goes left x
                    xStep = port.Length + _spaceBetweenPorts;
                    yStep = 0;
                }

                if (_side == BlockSideEnum.East)
                {
                    // goes down y
                    xStep = 0;
                    yStep = (port.Length + _spaceBetweenPorts) * -1;
                }

                if (_side == BlockSideEnum.South)
                {
                    // goes right x
                    xStep = (port.Length + _spaceBetweenPorts) * -1;
                    yStep = 0;
                }

                result.AddRange(port.CreateDiagramObjects(portX, portY, blockType));

                portX += xStep;
                portY += yStep;
            }


            return result;

        }

    }
}
