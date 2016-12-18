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
    public static class MapGenerator
    {
        /// <summary>
        /// Generates a randomized Map object based on the map generator's properties. 
        /// </summary>
        public static Map GenerateMap(MapParameters mapParameters)
        {
            if (mapParameters == null)
                throw new System.ArgumentNullException("mapParameters");

            var mapParams = mapParameters.Copy(true);

            return MapBuilder
                .InitializeRandomMap(mapParams.Length, mapParams.Width, mapParams.InitialDensity, mapParams.Seed)
                .Smooth()
                .RemoveSmallFloorRegions(mapParams.MinFloorSize)
                .ConnectFloors(mapParams.FloorExpansion)
                .ExpandRegions(mapParams.FloorExpansion)
                .RemoveSmallWallRegions(mapParams.MinWallSize)
                .Smooth()
                .ApplyBorder(mapParams.BorderSize);
        }
    } 
}