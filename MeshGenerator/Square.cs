using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshHelpers
{
    class Square
    {
        static List<int[]> configurationTable = new List<int[]>
        {
            new int[] { },
            new int[] {7, 5, 6 },
            new int[] {4, 5, 3 },
            new int[] {3, 4, 6, 7 },
            new int[] {2, 3, 1 },
            new int[] {1, 3, 5, 6, 7 },
            new int[] {1, 2, 4, 5 },
            new int[] {1, 2, 4, 6, 7 },
            new int[] {0, 1, 7 },
            new int[] {0, 1, 5, 6 },
            new int[] {0, 1, 3, 4, 5, 7 },
            new int[] {0, 1, 3, 4, 6 },
            new int[] {0, 2, 3, 7 },
            new int[] {0, 2, 3, 5, 6 },
            new int[] {0, 2, 4, 5, 7 },
            new int[] {0, 2, 4, 6 }
        };

        int configuration;
        Node[] nodes;

        internal Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
        {
            // The eight nodes in the square are indexed from 0 to 7 by starting at the topleft and going clockwise
            nodes = new Node[]
            {
                topLeft, topLeft.right, topRight, bottomRight.above, bottomRight, bottomLeft.right, bottomLeft, bottomLeft.above
            };

            configuration = 8 * Convert.ToInt32(topLeft.active) + 4 * Convert.ToInt32(topRight.active) +
                2 * Convert.ToInt32(bottomRight.active) + Convert.ToInt32(bottomLeft.active);
        }

        internal Node[] GetPoints()
        {
            int[] pointIndices = configurationTable[configuration];
            Node[] points = new Node[pointIndices.Length];
            for (int i = 0; i < pointIndices.Length; i++)
            {
                Node node = nodes[pointIndices[i]];
                points[i] = node;
            }
            return points;
        }
    }
}
