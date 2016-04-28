using System;
using System.Collections.Generic;

namespace MeshHelpers
{
    /// <summary>
    /// This class is responsible for triangulating the space occupied by a square of four points in the original map. In 
    /// addition to those four points, another four are introduced inbetween, for a total of eight. 
    /// </summary>
    class Square
    {
        /// <summary>
        /// The lookup table for the marching squares algorithm. The eight points in the square are enumerated from 0 to 7, 
        /// starting in the top left corner and going clockwise. Based on the sixteen possible configurations for a square,
        /// where each corner can be active or inactive, this table returns the points needed to triangulate that configuration. 
        /// </summary>
        static List<int[]> configurationTable = new List<int[]>
        {
            new int[] { },
            new int[] {5, 6, 7 },
            new int[] {3, 4, 5 },
            new int[] {3, 4, 6, 7 },
            new int[] {1, 2, 3 },
            new int[] {1, 2, 3, 5, 6, 7 },
            new int[] {1, 2, 4, 5 },
            new int[] {1, 2, 4, 6, 7 },
            new int[] {0, 1, 7 },
            new int[] {0, 1, 5, 6 },
            new int[] {0, 1, 3, 4, 5, 7 },
            new int[] {0, 1, 3, 4, 6 },
            new int[] {0, 2, 3, 7 },
            new int[] {0, 2, 3, 5, 6 },
            new int[] {0, 2, 4, 5, 7 },
            new int[] {0, 2, 4, 6}
        };

        /// <summary>
        /// A binary representation of the four corners of the square. 
        /// </summary>
        int configuration;
        Node[] nodes;

        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
        {
            nodes = new Node[]
            {
                topLeft, topLeft.right, topRight, bottomRight.above,
                    bottomRight, bottomLeft.right, bottomLeft, bottomLeft.above
            };

            configuration = 8 * Convert.ToInt32(topLeft.active)
                            + 4 * Convert.ToInt32(topRight.active) 
                            + 2 * Convert.ToInt32(bottomRight.active) 
                            + Convert.ToInt32(bottomLeft.active);
        }

        /// <summary>
        /// Get the points needed to triangulate a square in the original Map, based on the marching squares algorithm. 
        /// The resulting points should be triangulated in the following way: each triangle should start at the first point. 
        /// The second and third vertex in the triangle should be a pair of consecutive points. Every such combination should be
        /// triangulated. 
        /// 
        /// Example: If the returned array is [a, d, e, g, m], then the triangles should be (a, d, e), (a, e, g), (a, g, m).
        /// </summary>
        /// <returns>An array of points to be triangulated.</returns>
        public Node[] GetPoints()
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
