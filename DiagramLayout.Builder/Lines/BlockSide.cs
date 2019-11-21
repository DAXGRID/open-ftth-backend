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

        public bool CenterAlignment = false;

        private double _sideMargin = 40;
        public double SideMargin
        {
            get { return _sideMargin; }
            set { _sideMargin = value; }
        }

        private double _spaceBetweenPorts = 20;

        private List<BlockPort> _ports = new List<BlockPort>();
        private LineBlock _lineBlock = null;


        public BlockSide(LineBlock lineBlock, BlockSideEnum side)
        {
            _lineBlock = lineBlock;
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
            /*
            if (Side == BlockSideEnum.South || Side == BlockSideEnum.East)
                _ports.Reverse();
            */

            List<DiagramObject> result = new List<DiagramObject>();

            double portX = offsetX;
            double portY = offsetY;

            if (CenterAlignment && (_side == BlockSideEnum.North || _side == BlockSideEnum.South))
            {
                double totalPortLength = 0;

                foreach (var port in _ports)
                    totalPortLength += port.Length;

                double spaceLeft = _lineBlock.MinWidth - totalPortLength;

                double portSpace = spaceLeft / (_ports.Count + 1);
                _sideMargin = portSpace;
                _spaceBetweenPorts = portSpace;
            }

            if (_side == BlockSideEnum.Vest || _side == BlockSideEnum.East)
                portY += _sideMargin;
            else if (_side == BlockSideEnum.North || _side == BlockSideEnum.South)
                portX += _sideMargin;

            foreach (var port in _ports)
            {

                double xStep = 1;
                double yStep = 1;

                if (_side == BlockSideEnum.Vest || _side == BlockSideEnum.East)
                {
                    // goes up y
                    xStep = 0;
                    yStep = port.Length + _spaceBetweenPorts;
                }

                if (_side == BlockSideEnum.North || _side == BlockSideEnum.South)
                {
                    // goes left x
                    xStep = port.Length + _spaceBetweenPorts;
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
