// Uncomment this preprocessor directive to disable multithreading. 
// #define SINGLE_THREAD

/* The different generators have been broken up into separate, private, nested classes to cleanly divide
 the state and logic unique to each.*/

using System;
using System.Collections.Generic;
using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;
using CaveGeneration.Modules;
using UnityEngine;

namespace CaveGeneration
{
    public static class CaveGenerator
    {
        /// <summary>
        /// Generates a three tiered cave.
        /// </summary>
        /// <param name="randomizeSeeds">Will reroll the random seeds on each randomizable component.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static GameObject GenerateThreeTierCave(ThreeTierCaveConfiguration config, bool randomizeSeeds)
        {
            var generator = new ThreeTieredCaveGenerator();
            return generator.Generate(config, randomizeSeeds);
        }

        /// <summary>
        /// Generates a cave where the walls consist of rock prefabs instantiated along the outlines. Does not
        /// produce a ceiling.
        /// </summary>
        /// <param name="randomizeSeeds">Will reroll the random seeds on each randomizable component.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static GameObject GenerateRockCave(RockCaveConfiguration config, bool randomizeSeeds)
        {
            var generator = new RockCaveGenerator();
            return generator.Generate(config, randomizeSeeds);
        }

        static int GetRandomSeed()
        {
            return Guid.NewGuid().GetHashCode();
        }

        static void Execute(Action[] actions)
        {
#if SINGLE_THREAD
            Array.ForEach(actions, action => action.Invoke());
#else
            Threading.Threading.ParallelExecute(actions);
#endif
        }

        sealed class RockCaveGenerator
        {
            public GameObject Generate(RockCaveConfiguration config, bool randomizeSeeds)
            {
                if (config == null)
                    throw new ArgumentNullException("config");

                string message = config.Validate();
                if (message.Length > 0)
                    throw new ArgumentException(message, "config");

                if (randomizeSeeds)
                    config.SetSeed(GetRandomSeed());

                Map map                            = config.MapGenerator.Generate();
                IHeightMap heightMap               = config.HeightMapModule.GetHeightMap();
                IOutlinePrefabber outlinePrefabber = config.OutlineModule.GetOutlinePrefabber();
                Material floorMaterial             = config.Material;
                int scale                          = config.Scale;

                GameObject cave = new GameObject("Cave");

                Map[,] mapChunks = MapSplitter.Subdivide(map);
                mapChunks.ForEach((x, y) =>
                {
                    Coord index = new Coord(x, y);

                    WallGrid grid            = MapConverter.MapToWallGrid(mapChunks[x, y], scale, index);
                    List<Vector3[]> outlines = MeshGenerator.BuildOutlines(grid);
                    CaveMeshes caveMeshes    = BuildCaveMesh(grid, heightMap);
                    Sector sector            = BuildSector(caveMeshes, index, cave, floorMaterial);
                    GameObject rockAnchor    = BuildRockAnchor(sector.GameObject, index);
                    PlaceRocks(outlines, outlinePrefabber, rockAnchor.transform);
                });
               
                return cave;
            }

            /// <summary>
            /// The purpose of the rock anchor is to serve as a parent in the hierarchy to all the rocks for
            /// organizational purposes.
            /// </summary>
            GameObject BuildRockAnchor(GameObject parent, Coord index)
            {
                GameObject rockAnchor = new GameObject(string.Format("Rocks {0}", index));
                rockAnchor.SetParent(parent);
                return rockAnchor;
            }

            CaveMeshes BuildCaveMesh(WallGrid grid, IHeightMap heightMap)
            {
                MeshData floorPreMesh = MeshGenerator.BuildFloor(grid, heightMap);
                Mesh floorMesh = floorPreMesh.CreateMesh();
                return new CaveMeshes(floorMesh);
            }

            Sector BuildSector(CaveMeshes caveMeshes, Coord index, GameObject parent, Material mat)
            {
                Sector sector = new Sector(caveMeshes, index);
                sector.Floor.Material = mat;
                sector.GameObject.SetParent(parent);
                return sector;
            }

            void PlaceRocks(List<Vector3[]> outlines, IOutlinePrefabber outlinePrefabber, Transform parent)
            {
                foreach (var outline in outlines)
                {
                    outlinePrefabber.ProcessOutline(outline, parent);
                }
            }
        }

        sealed class ThreeTieredCaveGenerator
        {
            public GameObject Generate(ThreeTierCaveConfiguration config, bool randomizeSeeds)
            {
                if (config == null)
                    throw new ArgumentNullException("config");

                string message = config.Validate();
                if (message.Length > 0)
                    throw new ArgumentException(message, "config");

                if (randomizeSeeds)
                {
                    config.SetSeed(GetRandomSeed());
                }

                Map map            = config.MapGenerator.Generate();
                IHeightMap floor   = config.FloorHeightMapModule.GetHeightMap();
                IHeightMap ceiling = config.CeilingHeightMapModule.GetHeightMap();

                Map[,] mapChunks         = MapSplitter.Subdivide(map);
                CaveMeshes[,] caveChunks = GenerateCaveChunks(mapChunks, config.CaveType, config.Scale, floor, ceiling);
                ThreeTierCave cave       = new ThreeTierCave(caveChunks);
                AssignMaterials(cave, config.FloorMaterial, config.WallMaterial, config.CeilingMaterial);

                return cave.GameObject;
            }

            static CaveMeshes[,] GenerateCaveChunks(Map[,] mapChunks, ThreeTierCaveType type, int scale, IHeightMap floor, IHeightMap ceiling)
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
                        WallGrid grid        = MapConverter.MapToWallGrid(mapChunks[x, y], scale, index);
                        MeshData floorMesh   = MeshGenerator.BuildFloor(grid, floor);
                        MeshData ceilingMesh = SelectCeilingBuilder(type)(grid, ceiling);
                        MeshData wallMesh    = MeshGenerator.BuildWalls(grid, floor, ceiling);

                        caveChunks[index.x, index.y] = new CaveMeshes(floorMesh, wallMesh, ceilingMesh);
                    });
                });
                Execute(actions);
                return caveChunks;
            }

            static void AssignMaterials(ThreeTierCave cave, Material floorMat, Material wallMat, Material ceilingMat)
            {
                foreach (var floor in cave.GetFloors())
                {
                    floor.Material = floorMat;
                }
                foreach (var wall in cave.GetWalls())
                {
                    wall.Material = floorMat;
                }
                foreach (var ceiling in cave.GetCeilings())
                {
                    ceiling.Material = ceilingMat;
                }
            }

            static Func<WallGrid, IHeightMap, MeshData> SelectCeilingBuilder(ThreeTierCaveType caveType)
            {
                switch (caveType)
                {
                    case ThreeTierCaveType.Isometric:
                        return MeshGenerator.BuildCeiling;
                    case ThreeTierCaveType.Enclosed:
                        return MeshGenerator.BuildEnclosure;
                    default:
                        throw new System.ComponentModel.InvalidEnumArgumentException();
                }
            }
        }
    }
}
