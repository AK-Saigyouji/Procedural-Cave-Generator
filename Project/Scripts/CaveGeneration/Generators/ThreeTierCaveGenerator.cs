using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using AKSaigyouji.ArrayExtensions;
using AKSaigyouji.MeshGeneration;
using AKSaigyouji.HeightMaps;
using AKSaigyouji.Maps;
using AKSaigyouji.Modules.CaveWalls;

namespace AKSaigyouji.CaveGeneration
{
    sealed class ThreeTieredCaveGenerator : CaveGenerator
    {
        readonly MeshGenerator meshGenerator;
        readonly ThreeTierCaveConfiguration configuration;

        public ThreeTieredCaveGenerator(MeshGenerator meshGenerator, ThreeTierCaveConfiguration configuration)
        {
            if (meshGenerator == null)
                throw new ArgumentNullException("meshGenerator");

            if (configuration == null)
                throw new ArgumentNullException("configuration");

            string message = configuration.Validate();

            if (message.Length > 0)
                throw new ArgumentException(message, "configuration");

            this.meshGenerator = meshGenerator;
            this.configuration = configuration;
        }

        public override GameObject Generate()
        {
            Map map = configuration.MapGenerator.Generate();
            IHeightMap floor = configuration.FloorHeightMapModule.GetHeightMap();
            IHeightMap ceiling = configuration.CeilingHeightMapModule.GetHeightMap();
            CaveWallModule caveWalls = configuration.WallModule;

            Map[,] mapChunks = MapSplitter.Subdivide(map);
            CaveMeshes[,] caveChunks = GenerateCaveChunks(mapChunks, configuration.CaveType, configuration.Scale, floor, ceiling, caveWalls);
            ThreeTierCave cave = new ThreeTierCave(caveChunks);
            AssignMaterials(cave, configuration.FloorMaterial, configuration.WallMaterial, configuration.CeilingMaterial);

            return cave.GameObject;
        }

        CaveMeshes[,] GenerateCaveChunks(Map[,] mapChunks, ThreeTierCaveType type, int scale,
            IHeightMap floor, IHeightMap ceiling, CaveWallModule walls)
        {
            int xNumChunks = mapChunks.GetLength(0);
            int yNumChunks = mapChunks.GetLength(1);
            var caveChunks = new CaveMeshes[xNumChunks, yNumChunks];
            var actions = new Action[mapChunks.Length];
            mapChunks.ForEach((x, y) =>
            {
                Coord index = new Coord(x, y);
                actions[y * xNumChunks + x] = new Action(() =>
                {
                    WallGrid grid = MapConverter.MapToWallGrid(mapChunks[x, y], scale, index);
                    MeshData floorMesh = meshGenerator.BuildFloor(grid, floor);
                    MeshData ceilingMesh = SelectCeilingBuilder(type)(grid, ceiling);
                    MeshData wallMesh = meshGenerator.BuildWalls(grid, floor, ceiling, walls);

                    caveChunks[index.x, index.y] = new CaveMeshes(floorMesh, wallMesh, ceilingMesh);
                });
            });
            Execute(actions);
            return caveChunks;
        }

        void AssignMaterials(ThreeTierCave cave, Material floorMat, Material wallMat, Material ceilingMat)
        {
            foreach (var floor in cave.GetFloors())
            {
                floor.Material = floorMat;
            }
            foreach (var wall in cave.GetWalls())
            {
                wall.Material = wallMat;
            }
            foreach (var ceiling in cave.GetCeilings())
            {
                ceiling.Material = ceilingMat;
            }
        }

        Func<WallGrid, IHeightMap, MeshData> SelectCeilingBuilder(ThreeTierCaveType caveType)
        {
            switch (caveType)
            {
                case ThreeTierCaveType.Isometric:
                    return meshGenerator.BuildCeiling;
                case ThreeTierCaveType.Enclosed:
                    return meshGenerator.BuildEnclosure;
                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException();
            }
        }
    } 
}