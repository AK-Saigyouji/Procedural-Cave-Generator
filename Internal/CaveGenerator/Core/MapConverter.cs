/* This class converts the object produced by the MapGeneration system for consumption by systems that have no knowledge
of the MapGeneration system. */

using UnityEngine;
using System.Collections;
using CaveGeneration.MapGeneration;

using WallGrid = CaveGeneration.MeshGeneration.WallGrid;
using UnityEngine.Assertions;

namespace CaveGeneration
{
    /// <summary>
    /// Converts the output of the MapGenerator into forms suitable for consumption by other systems.
    /// </summary>
    static class MapConverter
    {
        /// <summary>
        /// Converts a map for consumption by the mesh generation system.
        /// </summary>
        public static WallGrid ToWallGrid(Map map, int scale)
        {
            Assert.IsNotNull(map);
            Assert.IsTrue(scale > 0, "Scale must be positive");

            Vector3 position = IndexToPosition(map.Index, scale);
            return new WallGrid(map.ToByteArray(), position, scale);
        }

        static Vector3 IndexToPosition(Coord index, int scale)
        {
            return new Vector3(index.x, 0f, index.y) * scale * MapSplitter.CHUNK_SIZE;
        }
    } 
}
