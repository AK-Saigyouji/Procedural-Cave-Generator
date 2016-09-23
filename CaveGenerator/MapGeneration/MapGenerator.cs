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