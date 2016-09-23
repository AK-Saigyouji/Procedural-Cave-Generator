﻿/* High level class overseeing the mesh generation system. The generate methods are all written in a thread-safe way, 
 * which is why a custom MeshData class is used instead of Unity's unsafe Mesh class. The Create methods are not thread-safe,
 * as they turn the MeshData classes into Meshes. Responsibility for generating individual meshes is delegated to specific
 * MeshBuilders. */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Produces meshes and colliders for Map objects. Break large maps into smaller maps before generating meshes.
    /// Maps should not be larger than 200 by 200.
    /// </summary>
    public sealed class MeshGenerator
    {
        MeshData ceilingMesh;
        MeshData wallMesh;
        MeshData floorMesh;
        MeshData enclosureMesh;

        IList<Outline> outlines;

        public Coord index { get; private set; }

        /// <summary>
        /// Generate the data necessary to produce meshes for isometric type cave. Generates ceiling, wall and floor meshes.
        /// </summary>
        public void GenerateIsometric(Map map, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            index = map.index;
            GenerateCeiling(map, ceilingHeightMap);
            ComputeMeshOutlines(ceilingMesh);
            GenerateWallsFromCeiling();
            GenerateFloor(map, floorHeightMap);
        }

        /// <summary>
        /// Generate the data necessary to produce meshes for enclosed cave. Generates floor, wall and enclosure meshes.
        /// </summary>
        public void GenerateEnclosed(Map map, IHeightMap floorHeightMap, IHeightMap enclosureHeightMap)
        {
            index = map.index;
            GenerateFloor(map, floorHeightMap);
            ComputeMeshOutlines(floorMesh);
            ReverseOutlines();
            GenerateEnclosure(enclosureHeightMap);
            GenerateWallsFromEnclosure();
            PruneWallsAtGlobalSeams(map.squareSize);
        }

        /// <summary>
        /// Get the mesh for the ceiling component. Check the docstring for the type of cave you generated to ensure
        /// it produces this type of mesh.
        /// </summary>
        public Mesh GetCeilingMesh()
        {
            return ceilingMesh.CreateMesh();
        }

        /// <summary>
        /// Get the mesh for the wall component. Check the docstring for the type of cave you generated to ensure
        /// it produces this type of mesh.
        /// </summary>
        public Mesh GetWallMesh()
        {
            return wallMesh.CreateMesh();
        }

        /// <summary>
        /// Get the mesh for the floor component. Check the docstring for the type of cave you generated to ensure
        /// it produces this type of mesh.
        /// </summary>
        public Mesh GetFloorMesh()
        {
            return floorMesh.CreateMesh();
        }

        /// <summary>
        /// Get the mesh for the enclosure component. Check the docstring for the type of cave you generated to ensure
        /// it produces this type of mesh.
        /// </summary>
        public Mesh GetEnclosureMesh()
        {
            return enclosureMesh.CreateMesh();
        }

        // Because of the fact that enclosed caves build walls around floors instead of walls, the seam between two
        // floored chunks have walls generated on them. This method removes those walls.
        void PruneWallsAtGlobalSeams(int scale)
        {
            int modulo = scale * Map.maxSubmapSize;
            WallPruner.PruneModulo(wallMesh, modulo);
        }

        void GenerateCeiling(Map map, IHeightMap ceilingHeightMap)
        {
            IMeshBuilder ceilingBuilder = new CeilingBuilder(map, ceilingHeightMap);
            ceilingMesh = ceilingBuilder.Build();
        }

        void GenerateWallsFromCeiling()
        {
            IMeshBuilder wallBuilder = new WallBuilder(ceilingMesh.vertices, outlines);
            wallMesh = wallBuilder.Build();
        }

        void GenerateWallsFromEnclosure()
        {
            IMeshBuilder wallBuilder = new WallBuilder(enclosureMesh.vertices, outlines);
            wallMesh = wallBuilder.Build();
        }

        void GenerateFloor(Map map, IHeightMap heightMap)
        {
            IMeshBuilder floorBuilder = new FloorBuilder(map, heightMap);
            floorMesh = floorBuilder.Build();
        }

        void GenerateEnclosure(IHeightMap heightMap)
        {
            IMeshBuilder enclosureBuilder = new EnclosureBuilder(floorMesh, heightMap);
            enclosureMesh = enclosureBuilder.Build();
        }

        void ComputeMeshOutlines(MeshData mesh)
        {
            OutlineGenerator outlineGenerator = new OutlineGenerator(mesh);
            outlines = outlineGenerator.GenerateOutlines();
        }

        void ReverseOutlines()
        {
            foreach (Outline outline in outlines)
            {
                outline.Reverse();
            }
        }
    }
}