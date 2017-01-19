﻿using System;
using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    public static class CaveGenerator
    {
        /// <param name="randomizeSeeds">Will reroll the random seeds on each randomizable component.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static Cave Generate(CaveConfiguration config, bool randomizeSeeds)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (config.MapGenerator == null)
                throw new ArgumentException("Config must contain a map generation module.");

            if (config.FloorHeightMapModule == null)
                throw new ArgumentException("Config must contain a height map module for the floor.");

            if (config.CeilingHeightMapModule == null)
                throw new ArgumentException("Config must contain a height map module for the ceiling.");

            if (randomizeSeeds)
                config.RandomizeSeeds();

            Map map = config.MapGenerator.Generate();
            IHeightMap floor = config.FloorHeightMapModule.GetHeightMap();
            IHeightMap ceiling = config.CeilingHeightMapModule.GetHeightMap();

            Map[,] mapChunks = MapSplitter.Subdivide(map);
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
