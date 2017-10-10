using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using AKSaigyouji.ArrayExtensions;
using AKSaigyouji.Maps;
using AKSaigyouji.MeshGeneration;
using AKSaigyouji.HeightMaps;
using AKSaigyouji.Modules.Outlines;

namespace AKSaigyouji.CaveGeneration
{
    sealed class OutlineCaveGenerator : CaveGenerator
    {
        readonly MeshGenerator meshGenerator;
        readonly RockCaveConfiguration configuration;

        public OutlineCaveGenerator(MeshGenerator meshGenerator, RockCaveConfiguration configuration)
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
            int scale = configuration.Scale;
            Map map = configuration.MapGenerator.Generate();
            Material floorMaterial = configuration.Material;
            IHeightMap heightMap = configuration.HeightMapModule.GetHeightMap();
            OutlineModule outlinePrefabber = configuration.OutlineModule;

            GameObject cave = new GameObject("Cave");

            Map[,] mapChunks = MapSplitter.Subdivide(map);
            mapChunks.ForEach((x, y) =>
            {
                Coord index = new Coord(x, y);

                WallGrid grid = MapConverter.MapToWallGrid(mapChunks[x, y], scale, index);
                List<Vector3[]> outlines = meshGenerator.BuildOutlines(grid);
                CaveMeshes caveMeshes = BuildCaveMesh(grid, heightMap);
                Sector sector = BuildSector(caveMeshes, index, cave, floorMaterial);
                GameObject rockAnchor = BuildRockAnchor(sector.GameObject, index);
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
}