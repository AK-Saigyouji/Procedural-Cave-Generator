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
using System.Linq;
using UnityEngine;

namespace AKSaigyouji.MeshGeneration
{
    sealed class OutlineGenerator
    {
        int length;
        int width;

        // a jagged array would be simpler here, but would require 640 bytes of overhead (from the 16 arrays)
        // to store about 30 bytes of actual data. Here we use up to three extra bytes per row versus the 40 for
        // an array's overhead. 
        readonly byte[,] outlineTable =      
        {
            // the first number tells us how many non-zero entries there are in that row.
            {0, 0, 0, 0, 0 }, //  0: empty
            {2, 7, 5, 0, 0 }, //  1: bottom-left triangle
            {2, 5, 3, 0, 0 }, //  2: bottom-right triangle
            {2, 7, 3, 0, 0 }, //  3: bottom half
            {2, 3, 1, 0, 0 }, //  4: top-right triangle
            {4, 7, 1, 3, 5 }, //  5: all but top-left and bottom-right triangles
            {2, 5, 1, 0, 0 }, //  6: right half
            {2, 7, 1, 0, 0 }, //  7: all but top-left triangle
            {2, 1, 7, 0, 0 }, //  8: top-left triangle
            {2, 1, 5, 0, 0 }, //  9: left half
            {4, 5, 7, 1, 3 }, // 10: all but bottom-left and top-right
            {2, 1, 3, 0, 0 }, // 11: all but top-right
            {2, 3, 7, 0, 0 }, // 12: top half
            {2, 3, 5, 0, 0 }, // 13: all but bottom-right
            {2, 5, 7, 0, 0 }, // 14: all but bottom-left
            {0, 0, 0, 0, 0 }  // 15: full square
        };

        // cached collections to reduce allocations.
        TwoWayLookup cachedLookup = new TwoWayLookup();
        List<Vector3[]> cachedOutlines = new List<Vector3[]>();
        HashSet<LocalPosition> cachedVisited = new HashSet<LocalPosition>();

        public List<Vector3[]> Generate(WallGrid grid)
        {
            byte[,] configurations = MarchingSquares.ComputeConfigurations(grid);
            length = configurations.GetLength(0);
            width = configurations.GetLength(1);
            TwoWayLookup outlineLookup = CreateLookupTable(configurations, outlineTable);

            var outlines = cachedOutlines;
            outlines.Clear();
            var visited = cachedVisited;
            visited.Clear();
            foreach (var pair in outlineLookup)
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
                    Vector3[] completeOutline = outline.Select(p => p.ToGlobalPosition(grid.Scale, grid.Position)).ToArray();
                    outlines.Add(completeOutline);
                }
            }
            return outlines;
        }

        static void AddToOutline(List<LocalPosition> outline, LocalPosition item, HashSet<LocalPosition> visited)
        {
            visited.Add(item);
            outline.Add(item);
        }

        TwoWayLookup CreateLookupTable(byte[,] configurations, byte[,] outlineTable)
        {
            cachedLookup.Clear();
            var lookupTable = cachedLookup;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    int configuration = configurations[x, y];
                    int rowLength = outlineTable[configuration, 0];
                    for (int i = 1; i < rowLength; i += 2)
                    {
                        var a = new LocalPosition(x, y, outlineTable[configuration, i]);
                        var b = new LocalPosition(x, y, outlineTable[configuration, i + 1]);

                        lookupTable.AddPair(a, b);
                    }
                }
            }
            return lookupTable;
        }

        private sealed class TwoWayLookup
        {
            readonly Dictionary<LocalPosition, LocalPosition> forwardLookup;
            readonly Dictionary<LocalPosition, LocalPosition> backwardLookup;

            public TwoWayLookup(int capacity = 10)
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

            /// <summary>
            /// The enumerator will return pairs backwards, i.e. in (end, start) form. 
            /// </summary>
            public Dictionary<LocalPosition, LocalPosition>.Enumerator GetEnumerator()
            {
                return backwardLookup.GetEnumerator();
            }

            public void Clear()
            {
                forwardLookup.Clear();
                backwardLookup.Clear();
            }
        }

    }
}
