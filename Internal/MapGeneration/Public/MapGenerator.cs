#if UNITY_EDITOR
using Stopwatch = System.Diagnostics.Stopwatch;
using CaveGeneration.Utility;
#endif

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Generates a randomized cave-like Map object with the property that every floor tile is reachable from every other
    /// floor tile. The outermost boundary of the map consists of wall tiles.
    /// </summary>
    public sealed class MapGenerator : IMapGenerator
    {
        MapParameters map;

        public MapGenerator(MapParameters parameters)
        {
            if (parameters == null) throw new System.ArgumentNullException("parameters", "MapParameters must not be null!");
            map = parameters;
        }

        /// <summary>
        /// Generates a randomized Map object based on the map generator's properties. May take a significant amount of time
        /// for large maps (particularly for width * length > 1e6). 
        /// </summary>
        public Map GenerateMap()
        {
            MapBuilder builder = new MapBuilder(map.Length, map.Width, map.SquareSize);
            builder.InitializeRandomFill(map.InitialDensity, map.Seed);
            builder.Smooth();
            builder.RemoveSmallFloorRegions(map.MinFloorSize);
            builder.ConnectFloors(map.FloorExpansion);
            builder.ExpandRegions(map.FloorExpansion);
            builder.RemoveSmallWallRegions(map.MinWallSize);
            builder.ApplyBorder(map.BorderSize);
            return builder.ToMap();
        }
    } 
}