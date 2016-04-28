using UnityEngine;

namespace MeshHelpers
{
    /// <summary>
    /// The SquareGrid creates and stores the Squares used in the marching squares algorithm.
    /// </summary>
    class SquareGrid
    {
        Square[,] squares;

        public SquareGrid(Map map)
        {
            int nodeCountX = map.length;
            int nodeCountY = map.width;
            ControlNode[,] controlNodes = CreateControlNodes(nodeCountX, nodeCountY, map);
            squares = CreateSquares(nodeCountX, nodeCountY, controlNodes);
        }

        /// <summary>
        /// For each point in the original Map, a ControlNode is created. If it was a wall it's active, otherwise it's inactive.
        /// </summary>
        /// <param name="nodeCountX"></param>
        /// <param name="nodeCountY"></param>
        /// <param name="map"></param>
        /// <returns>A grid of ControlNodes, one for each point in the original Map.</returns>
        ControlNode[,] CreateControlNodes(int nodeCountX, int nodeCountY, Map map)
        {
            Vector3 positionOffset = new Vector3(map.position.x, 0f, map.position.y);
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 position = new Vector3(x, 0f, y) * map.squareSize + positionOffset;
                    bool nodeActive = map[x, y] == 1;
                    controlNodes[x, y] = new ControlNode(position, nodeActive, map.squareSize);
                }
            }
            return controlNodes;
        }

        Square[,] CreateSquares(int nodeCountX, int nodeCountY, ControlNode[,] controlNodes)
        {
            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], 
                                                controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
            return squares;
        }

        public Square this[int x, int y]
        {
            get { return squares[x, y]; }
            private set { squares[x, y] = value; }
        }

        public int GetLength(int axis)
        {
            return squares.GetLength(axis);
        }
    }
}
