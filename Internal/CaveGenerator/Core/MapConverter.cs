/* This class converts the object produced by the MapGeneration system for consumption by systems that have no knowledge
of the MapGeneration system. */

using UnityEngine;
using System.Collections;
using CaveGeneration.MapGeneration;

using WallGrid = CaveGeneration.MeshGeneration.WallGrid;

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
        public static WallGrid ToWallGrid(Map map)
        {
            return new WallGrid(map.ToByteArray(), map.Position, map.SquareSize);
        }
    } 
}
