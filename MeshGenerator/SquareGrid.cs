using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshHelpers
{
    class SquareGrid
    {
        Square[,] squares;

        internal SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            ControlNode[,] controlNodes = CreateControlNodes(nodeCountX, nodeCountY, squareSize, map);
            squares = CreateSquares(nodeCountX, nodeCountY, controlNodes);
        }

        ControlNode[,] CreateControlNodes(int nodeCountX, int nodeCountY, float squareSize, int[,] map)
        {
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
            for (int x = 0; x < nodeCountX; x++)
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 position = new Vector3(x - mapWidth / 2, 0f, y - mapHeight / 2) * squareSize;
                    controlNodes[x, y] = new ControlNode(position, map[x, y] == 1, squareSize);
                }
            return controlNodes;
        }

        Square[,] CreateSquares(int nodeCountX, int nodeCountY, ControlNode[,] controlNodes)
        {
            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
                for (int y = 0; y < nodeCountY - 1; y++)
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
            return squares;
        }

        internal Square this[int x, int y]
        {
            get { return squares[x, y]; }
            private set { squares[x, y] = value; }
        }

        internal int GetLength(int axis)
        {
            return squares.GetLength(axis);
        }
    }
}
