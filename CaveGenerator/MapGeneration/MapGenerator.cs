using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Generates a randomized cave-like Map object with the property that every floor tile is reachable from every other
    /// floor tile. The outermost boundary of the map consists of wall tiles.
    /// </summary>
    public class MapGenerator : IMapGenerator
    {
        MapParameters map;

        public MapGenerator(MapParameters parameters)
        {
            map = parameters;
        }

        /// <summary>
        /// Generates a randomized Map object based on the map generator's properties. May take a significant amount of time
        /// for large maps (particularly for width * length > 10e6). 
        /// </summary>
        /// <returns>Returns the generated Map object</returns>
        public Map GenerateMap()
        {
            MapBuilder builder = new MapBuilder(map.Length, map.Width, map.SquareSize);
            builder.RandomFill(map.InitialDensity, GetSeed());
            builder.Smooth();
            builder.RemoveSmallFloorRegions(map.MinFloorSize);
            builder.ConnectFloors(1 + map.FloorExpansion);
            builder.ExpandRegions(map.FloorExpansion);
            builder.RemoveSmallWallRegions(map.MinWallSize);
            builder.ApplyBorder(map.BorderSize);
            return builder.ToMap();
        }

        /// <summary>
        /// Get the seed, which determines which map gets generated. Same seed will generate the same map. Different seeds
        /// will generate unpredictably different maps.
        /// </summary>
        int GetSeed()
        {
            if (map.UseRandomSeed)
            {
                return System.Environment.TickCount;
            }
            else
            {
                return map.Seed.GetHashCode();
            }
        }
    } 
}