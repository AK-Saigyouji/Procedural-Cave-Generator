using System;
using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    static class CaveGenerator
    {
        /// <param name="randomizeSeeds">Will reroll the random seeds on each randomizable component.</param>
        public static Cave Generate(CaveConfiguration config, bool randomizeSeeds)
        {
            if (config.MapGenerator == null)
                throw new InvalidOperationException("Must assign Map Generator before generating.");

            if (config.FloorHeightMap == null)
                throw new InvalidOperationException("Must assign Floor Height Map Component before generating.");

            if (config.CeilingHeightMap == null)
                throw new InvalidOperationException("Must assign Ceiling Height Map Component before generating.");

            if (randomizeSeeds)
                config.RandomizeSeeds();

            Map map = config.MapGenerator.Generate();
            IHeightMap floor = config.FloorHeightMap.GetHeightMap();
            IHeightMap ceiling = config.CeilingHeightMap.GetHeightMap();

            var mapChunks = MapSplitter.Subdivide(map);
            int xNumChunks = mapChunks.GetLength(0);
            int yNumChunks = mapChunks.GetLength(1);
            var caveChunks = new CaveMeshes[xNumChunks, yNumChunks];
            var actions = new Action[mapChunks.Length];
            for (int y = 0; y < yNumChunks; y++)
            {
                for (int x = 0; x < xNumChunks; x++)
                {
                    int xCopy = x, yCopy = y;
                    actions[y * xNumChunks + x] = new Action(() =>
                    {
                        WallGrid wallGrid = MapConverter.ToWallGrid(mapChunks[xCopy, yCopy], config.Scale, new Coord(xCopy, yCopy));
                        caveChunks[xCopy, yCopy] = MeshGenerator.Generate(wallGrid, config.CaveType, floor, ceiling);
                    });
                }
            }
            Utility.Threading.ParallelExecute(actions);
            Cave cave = new Cave(caveChunks, config);
            return cave;
        }
    }
}
