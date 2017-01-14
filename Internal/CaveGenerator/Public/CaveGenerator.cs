using System;
using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    static class CaveGenerator
    {
        public static Cave Generate(CaveConfiguration config)
        {
            if (config.MapGenerator == null)
                throw new InvalidOperationException("Must assign Map Generator before generating.");

            if (config.FloorHeightMap == null)
                throw new InvalidOperationException("Must assign Floor Height Map Component before generating.");

            if (config.CeilingHeightMap == null)
                throw new InvalidOperationException("Must assign Ceiling Height Map Component before generating.");

            Map map = config.MapGenerator.Generate();
            IHeightMap floor = config.FloorHeightMap.GetHeightMap();
            IHeightMap ceiling = config.CeilingHeightMap.GetHeightMap();

            MapChunk[] mapChunks = MapSplitter.Subdivide(map);
            var caveChunks = new CaveMeshChunk[mapChunks.Length];
            var actions = new Action[mapChunks.Length];
            for (int i = 0; i < mapChunks.Length; i++)
            {
                int iCopy = i; // Can't use i directly in the body of the action due to the way it's captured.

                MapChunk mapChunk = mapChunks[i];
                actions[i] = new Action(() =>
                {
                    WallGrid wallGrid = MapConverter.ToWallGrid(mapChunk, config.Scale);
                    CaveMeshes meshes = MeshGenerator.Generate(wallGrid, config.CaveType, floor, ceiling);
                    caveChunks[iCopy] = new CaveMeshChunk(meshes, mapChunk.Index);
                });
            }
            Utility.Threading.ParallelExecute(actions);

            Cave cave = new Cave(caveChunks, config);
            return cave;
        }
    }
}
