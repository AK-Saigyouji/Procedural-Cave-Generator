/* This class creates a list of outlines based on a marching square triangulation for a grid. 
 
The algorithm proceeds in two steps. The first step is to create a pair of lookup tables for 
all the outline vertices from each square based on its marching squares configuration. The forward lookup table
associates to each vertex the next vertex in the outline. The backward lookup table is the same but goes backwards.
Forward means if travelling from a to b, the interior of the outline will be to the right: direction is important as it
will determine from which direction the wall will be visible. 

Once the lookup tables are populated, the second step is to iterate over all vertices, and follow each outline to the 
end, maintaining a table of visited vertices to avoid duplicating outlines. There are two complications.
The first is that we might start in the middle of an outline, not necessarily the beginning. The second is that some
outlines will loop back to themselves, while others will not. 

To handle both complications, perform the following steps: 
First, travel backwards along the outline until reaching the starting point or running out of vertices in the outline.
Second, reverse the points discovered so far.
Third, check if the starting point was reached. If so, terminate. Otherwise, travel fowards along the outline
from the starting point to recover the remaining points. 

The biggest room for optimization has to do with the choice of data structures. Two dictionaries are used for the 
lookup tables and a hashset is used to keep track of visited points. C# implementations of these structures are very
heavyweight, and might be sped up dramatically by careful use of arrays with manually managed integral indices.
Positions in this algorithm are represented by a struct that admits of a natural, unique index. 
*/

using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    static class OutlineGenerator
    {
        // This gives us the squarepoints corresponding to the outlines in each square.
        // e.g. configuration 1 gives us just the bottom-right triangle, which has one outline edge
        // running from point 3 (mid-right) to 5 (down-mid). The order is always such that
        // the triangle is to the right of the edge when travelling from the first point to the second.
        static readonly byte[][] outlineTable = new byte[][]
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
            TwoWayLookup outlineLookup = CreateLookupTable(numOutlineEdges, configurations);

            var outlines = new List<Vector3[]>();
            var visited = new HashSet<LocalPosition>();
            foreach (var pair in outlineLookup.EnumerateBackwards())
            {
                if (!visited.Contains(pair.Key))
                {
                    LocalPosition start = pair.Key;
                    LocalPosition next = pair.Value;
                    var outline = new List<LocalPosition>();
                    AddToOutline(outline, start, visited);
                    AddToOutline(outline, next, visited);
                    // first do a backward pass until looping or running out of connected outline edges
                    while (start != next && outlineLookup.TryGetBackwardValue(next, out next))
                    {
                        AddToOutline(outline, next, visited);
                    }
                    outline.Reverse();
                    // if no loop, then do a forward pass from the starting point
                    if (start != next)
                    {
                        next = start;
                        while (outlineLookup.TryGetForwardValue(next, out next))
                        {
                            AddToOutline(outline, next, visited);
                        }
                    }
                    outlines.Add(ToGlobalPositions(outline, grid.Scale, grid.Position));
                }
            }
            return outlines;
        }

        static void AddToOutline(List<LocalPosition> outline, LocalPosition item, HashSet<LocalPosition> visited)
        {
            visited.Add(item);
            outline.Add(item);
        }

        static int CountOutlineEdges(byte[,] configurations)
        {
            int length = configurations.GetLength(0);
            int width = configurations.GetLength(1);
            int numOutlinePoints = 0;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    // Each square has 1 or 2 outlines running through it. This corresponds to 2 or 4 points in the array.
                    numOutlinePoints += outlineTable[configurations[x, y]].Length;
                }
            }
            return numOutlinePoints / 2;
        }

        static TwoWayLookup CreateLookupTable(int capacity, byte[,] configurations)
        {
            var lookupTable = new TwoWayLookup(capacity);
            int length = configurations.GetLength(0);
            int width = configurations.GetLength(1);
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    byte[] outlineData = outlineTable[configurations[x, y]];
                    for (int i = 0; i < outlineData.Length; i += 2)
                    {
                        var a = new LocalPosition(x, y, outlineData[i]);
                        var b = new LocalPosition(x, y, outlineData[i + 1]);

                        lookupTable.AddPair(a, b);
                    }
                }
            }
            return lookupTable;
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

        sealed class TwoWayLookup
        {
            readonly Dictionary<LocalPosition, LocalPosition> forwardLookup;
            readonly Dictionary<LocalPosition, LocalPosition> backwardLookup;

            public TwoWayLookup(int capacity)
            {
                forwardLookup = new Dictionary<LocalPosition, LocalPosition>(capacity);
                backwardLookup = new Dictionary<LocalPosition, LocalPosition>(capacity);
            }

            public void AddPair(LocalPosition start, LocalPosition end)
            {
                forwardLookup[start] = end;
                backwardLookup[end] = start;
            }

            public bool TryGetForwardValue(LocalPosition key, out LocalPosition value)
            {
                return forwardLookup.TryGetValue(key, out value);
            }

            public bool TryGetBackwardValue(LocalPosition key, out LocalPosition value)
            {
                return backwardLookup.TryGetValue(key, out value);
            }

            public IEnumerable<KeyValuePair<LocalPosition, LocalPosition>> EnumerateForwards()
            {
                foreach (var pair in forwardLookup)
                {
                    yield return pair;
                }
            }

            public IEnumerable<KeyValuePair<LocalPosition, LocalPosition>> EnumerateBackwards()
            {
                foreach (var pair in backwardLookup)
                {
                    yield return pair;
                }
            }
        }

    }
}
