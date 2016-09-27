/* This class bridges the MapGeneration and MeshGeneration interfaces by converting the output of the MapGenerator into
 * a form the MeshGenerator understands. */

using UnityEngine;
using System.Collections;
using CaveGeneration.MapGeneration;

using WallGrid = CaveGeneration.MeshGeneration.WallGrid;

namespace CaveGeneration
{
    /// <summary>
    /// Converts the output of the MapGenerator into a form suitable for the input for the MeshGenerator.
    /// </summary>
    static class MapConverter
    {
        public static WallGrid Convert(Map map)
        {
            return new WallGrid(map.ToByteArray(), map.Position, map.SquareSize);
        }
    } 
}
