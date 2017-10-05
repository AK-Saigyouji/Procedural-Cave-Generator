// Uncomment this preprocessor directive to disable multithreading. 
// #define SINGLE_THREAD

/* The different generators have been broken up into separate, private, nested classes to cleanly divide
 the state and logic unique to each.*/

using AKSaigyouji.ArrayExtensions;
using AKSaigyouji.HeightMaps;
using AKSaigyouji.Maps;
using AKSaigyouji.MeshGeneration;
using AKSaigyouji.Modules.Outlines;
using AKSaigyouji.Modules.CaveWalls;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AKSaigyouji.CaveGeneration
{
    public sealed class CaveGenerator
    {
        readonly MeshGenerator meshGenerator;

        public CaveGenerator()
        {
            meshGenerator = new MeshGenerator();
        }

        /// <summary>
        /// Generates a three tiered cave.
        /// </summary>
        /// <param name="randomizeSeeds">Will reroll the random seeds on each randomizable component.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public GameObject GenerateThreeTierCave(ThreeTierCaveConfiguration config, bool randomizeSeeds)
        {
            var generator = new ThreeTieredCaveGenerator(meshGenerator);
            return generator.Generate(config, randomizeSeeds);
        }

        /// <summary>
        /// Generates a cave where the walls consist of rock prefabs instantiated along the outlines. Does not
        /// produce a ceiling.
        /// </summary>
        /// <param name="randomizeSeeds">Will reroll the random seeds on each randomizable component.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public GameObject GenerateRockCave(RockCaveConfiguration config, bool randomizeSeeds)
        {
            var generator = new RockCaveGenerator(meshGenerator);
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
            readonly MeshGenerator meshGenerator;

            public RockCaveGenerator(MeshGenerator meshGenerator)
            {
                this.meshGenerator = meshGenerator;
            }

            public GameObject Generate(RockCaveConfiguration config, bool randomizeSeeds)
            {
                if (config == null)
                    throw new ArgumentNullException("config");

                string message = config.Validate();
                if (message.Length > 0)
                    throw new ArgumentException(message, "config");

                if (randomizeSeeds)
                    config.SetSeed(GetRandomSeed());

                int scale = config.Scale;
                Map map = config.MapGenerator.Generate();
                Material floorMaterial = config.Material;
                IHeightMap heightMap = config.HeightMapModule.GetHeightMap();
                OutlineModule outlinePrefabber = config.OutlineModule;

                GameObject cave = new GameObject("Cave");

                Map[,] mapChunks = MapSplitter.Subdivide(map);
                mapChunks.ForEach((x, y) =>
                {
                    Coord index = new Coord(x, y);

                    WallGrid grid            = MapConverter.MapToWallGrid(mapChunks[x, y], scale, index);
                    List<Vector3[]> outlines = meshGenerator.BuildOutlines(grid);
                    CaveMeshes caveMeshes    = BuildCaveMesh(grid, heightMap);
                    Sector sector            = BuildSector(caveMeshes, index, cave, floorMaterial);
                    GameObject rockAnchor    = BuildRockAnchor(sector.GameObject, index);
                    BuildOutline(outlines, outlinePrefabber, rockAnchor.transform);
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
                rockAnchor.transform.parent = parent.transform;
                return rockAnchor;
            }

            CaveMeshes BuildCaveMesh(WallGrid grid, IHeightMap heightMap)
            {
                MeshData floorPreMesh = meshGenerator.BuildFloor(grid, heightMap);
                Mesh floorMesh = floorPreMesh.CreateMesh();
                return new CaveMeshes(floorMesh);
            }

            void BuildOutline(IEnumerable<Vector3[]> outlineVertices, OutlineModule prefabber, Transform parent)
            {
                prefabber.ProcessOutlines(outlineVertices.Select(vertices => new Outline(vertices)), parent);
            }

            Sector BuildSector(CaveMeshes caveMeshes, Coord index, GameObject parent, Material mat)
            {
                Sector sector = new Sector(caveMeshes, index.x, index.y);
                sector.Floor.Material = mat;
                sector.GameObject.transform.parent = parent.transform;
                return sector;
            }
        }

        sealed class ThreeTieredCaveGenerator
        {
            readonly MeshGenerator meshGenerator;

            public ThreeTieredCaveGenerator(MeshGenerator meshGenerator)
            {
                this.meshGenerator = meshGenerator;
            }

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
                CaveWallModule caveWalls = CaveWallModule.Default;

                Map[,] mapChunks         = MapSplitter.Subdivide(map);
                CaveMeshes[,] caveChunks = GenerateCaveChunks(mapChunks, config.CaveType, config.Scale, floor, ceiling, caveWalls);
                ThreeTierCave cave       = new ThreeTierCave(caveChunks);
                AssignMaterials(cave, config.FloorMaterial, config.WallMaterial, config.CeilingMaterial);

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
                        WallGrid grid        = MapConverter.MapToWallGrid(mapChunks[x, y], scale, index);
                        MeshData floorMesh   = meshGenerator.BuildFloor(grid, floor);
                        MeshData ceilingMesh = SelectCeilingBuilder(type)(grid, ceiling);
                        MeshData wallMesh    = meshGenerator.BuildWalls(grid, floor, ceiling, walls);

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
}
