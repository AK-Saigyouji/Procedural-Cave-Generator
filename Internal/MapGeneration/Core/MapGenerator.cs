namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Generates a randomized cave-like Map object with the property that every floor tile is reachable from every other
    /// floor tile. The outermost boundary of the map consists of wall tiles.
    /// </summary>
    public static class MapGenerator
    {
        const int BASE_TUNNEL_RADIUS = 1;

        /// <summary>
        /// Generates a randomized Map object based on the map generator's properties. 
        /// </summary>
        public static Map GenerateMap(MapParameters mapParams)
        {
            if (mapParams == null)
                throw new System.ArgumentNullException("mapParams");

            return MapBuilder
                .InitializeRandomMap(mapParams.Length, mapParams.Width, mapParams.InitialDensity, mapParams.Seed)
                .Smooth()
                .RemoveSmallFloorRegions(mapParams.MinFloorSize)
                .ExpandRegions(mapParams.FloorExpansion)
                .ConnectFloors(tunnelRadius: BASE_TUNNEL_RADIUS + mapParams.FloorExpansion)
                .SmoothOnlyWalls()
                .RemoveSmallWallRegions(mapParams.MinWallSize)
                .ApplyBorder(mapParams.BorderSize);
        }
    } 
}