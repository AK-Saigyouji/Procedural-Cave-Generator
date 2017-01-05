/* This class creates a list of outlines based on a marching square triangulation for a grid. */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace CaveGeneration.MeshGeneration
{
    static class OutlineGenerator
    {
        // This gives us the squarepoints corresponding to the outlines in each square.
        // e.g. configuration 1 gives us just the bottom-right triangle, which has one outline edge
        // running from point 3 (mid-right) to 5 (down-mid). The order is always such that
        // the triangle is to the right of the edge when travelling from the first point to the second.
        static byte[][] outlineTable = new byte[][]
        {
            new byte[] { },           //  0: empty
            new byte[] {7, 5 },       //  1: bottom-left triangle
            new byte[] {5, 3 },       //  2: bottom-right triangle
            new byte[] {7, 3 },       //  3: bottom half
            new byte[] {3, 1 },       //  4: top-right triangle
            new byte[] {7, 1, 3, 5 }, //  5: all but top-left and bottom-right triangles
            new byte[] {5, 1 },       //  6: right half
            new byte[] {7, 1 },       //  7: all but top-left triangle
            new byte[] {1, 7 },       //  8: top-left triangle
            new byte[] {1, 5 },       //  9: left half
            new byte[] {5, 7, 1, 3 }, // 10: all but bottom-left and top-right
            new byte[] {1, 3 },       // 11: all but top-right
            new byte[] {3, 7 },       // 12: top half
            new byte[] {3, 5 },       // 13: all but bottom-right
            new byte[] {5, 7 },       // 14: all but bottom-left
            new byte[] {}             // 15: full square
        };

        public static List<Vector3[]> Generate(WallGrid grid)
        {
            byte[,] configurations = MarchingSquares.ComputeConfigurations(grid);
            int numOutlineEdges = CountOutlineEdges(configurations);
            var forwardLookup  = new Dictionary<LocalPosition, LocalPosition>(numOutlineEdges);
            var backwardLookup = new Dictionary<LocalPosition, LocalPosition>(numOutlineEdges);
            PopulateLookupTables(forwardLookup, backwardLookup, configurations);

            var outlines = new List<Vector3[]>();
            var visited = new HashSet<LocalPosition>();
            var reusableList = new List<LocalPosition>(backwardLookup.Count);
            foreach (var pair in backwardLookup)
            {
                if (!visited.Contains(pair.Key))
                {
                    LocalPosition start = pair.Key;
                    LocalPosition next = pair.Value;
                    reusableList.Clear();
                    List<LocalPosition> outline = reusableList;
                    outline.Add(start);
                    visited.Add(start);
                    outline.Add(next);
                    visited.Add(next);
                    // first we do a backward pass until we loop or until we run out of connected outline edges
                    while (start != next && backwardLookup.TryGetValue(next, out next))
                    {
                        outline.Add(next);
                        visited.Add(next);
                    }
                    outline.Reverse();
                    // if we didn't loop, then we need to do a forward pass from the starting point
                    if (start != next) 
                    {
                        next = start;
                        while (forwardLookup.TryGetValue(next, out next))
                        {
                            visited.Add(next);
                            outline.Add(next);
                        }
                    }
                    outlines.Add(ToGlobalPositions(outline, grid.Scale, grid.Position));
                }
            }
            return outlines;
        }

        static int CountOutlineEdges(byte[,] configurations)
        {
            int length = configurations.GetLength(0);
            int width = configurations.GetLength(1);
            int acc = 0;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    // Each square has either 4 or 2 points, correspondinding to 2 or 1 outlines.
                    acc += outlineTable[configurations[x, y]].Length;
                }
            }
            return acc / 2;
        }

        static void PopulateLookupTables(
            Dictionary<LocalPosition, LocalPosition> forward, 
            Dictionary<LocalPosition, LocalPosition> backward,
            byte[,] configurations)
        {
            int length = configurations.GetLength(0);
            int width = configurations.GetLength(1);
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    byte[] outlineData = outlineTable[configurations[x, y]];
                    for (int i = 0; i < outlineData.Length; i += 2)
                    {
                        LocalPosition a = new LocalPosition(x, y, outlineData[i]);
                        LocalPosition b = new LocalPosition(x, y, outlineData[i + 1]);

                        forward[a] = b;
                        backward[b] = a;
                    }
                }
            }
        }

        static Vector3[] ToGlobalPositions(List<LocalPosition> localPositions, int scale, Vector3 basePosition)
        {
            var vertices = new Vector3[localPositions.Count];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = localPositions[i].ToGlobalPosition(scale, basePosition);
            }
            return vertices;
        }
    }
}
