﻿/* High level class overseeing the mesh generation system. The generate methods are all written in a thread-safe way, 
 * which is why a custom MeshData class is used instead of Unity's unsafe Mesh class. The Create methods are not thread-safe,
 * as they turn the MeshData classes into Meshes. Responsibility for generating individual meshes is delegated to specific
 * MeshBuilders.
 * 
 * The core algorithm driving mesh generation is Marching Squares, which is implemented in the MapTriangulator class. 
 * It turns a grid of 0s (floors) and 1s (walls) into a collection of triangles representing the walls, but with more 
 * structure than simply putting a square at every 1. 
 * 
 * At the moment this class supports Enclosed and Isometric cave types. Isometric caves are designed with an isometric perspective
 * in mind (hence the name) and thus have ceilings that are built over the walls, not the floors. Enclosed caves are completely 
 * closed off caves designed with a 1st person perspective in mind.
 * 
 * For isometric cave generation, the 1s are triangulated into a ceiling, and the 0s are triangulated into a floor. 
 * Outlines of the ceiling are computed and quads are built to connect the ceiling and floor meshes, giving complete 3D 
 * geometry. Optional height maps then give the geometry added variation by translating the height of floors and ceilings. 
 * 
 * Enclosed cave generation is similar, but instead of triangulating a ceiling, a copy of the floor is made and inverted.
 * Outlines must be inverted since walls face inward for the enclosed caves, rather than outward for the isometric cave.
 */

using UnityEngine;
using UnityEngine.Assertions;

namespace CaveGeneration.MeshGeneration
{
    public enum CaveType
    {
        Isometric,
        Enclosed
    }

    /// <summary>
    /// Produces meshes and colliders for grids. Break grids larger than 200 by 200 into smaller grids before feeding
    /// into a mesh generator.
    /// </summary>
    public sealed class MeshGenerator
    {
        MeshData ceilingMesh;
        MeshData wallMesh;
        MeshData floorMesh;

        Outline[] outlines;

        public string Index { get; private set; }

        int? chunkSize; 

        /// <summary>
        /// If breaking up a single grid into multiple chunks, set chunksize properly to ensure meshes can be stitched 
        /// together correctly.
        /// </summary>
        /// <param name="ChunkSize">If breaking up a single grid into multiple square pieces, this number corresponds
        /// to their size (location of seams), and ensures walls are generated correctly along seams. Example: if breaking a 280 by 260 grid 
        /// into four pieces of size 150 by 150 (bottom left), 130 by 150 (bottom right), 150 by 110 (top left), and 
        /// 130 by 110 (top right), chunk size should be set to 150 as the seams run along x = 150 and y = 150. Leave null
        /// if not chunking a grid.</param>
        /// <param name="Index">Optional label, useful if using multiple mesh generators concurrently.</param>
        public MeshGenerator(int? ChunkSize, string Index)
        {
            chunkSize = ChunkSize;
            this.Index = Index ?? "";
        }

        /// <summary>
        /// Create a meshgenerator for an independent grid. If breaking a grid into multiple chunks, use constructor with 
        /// chunksize instead.
        /// </summary>
        public MeshGenerator() : this(null, null) { }

        /// <summary>
        /// Generate the data necessary to produce meshes for a cave in a manner that is safe to run outside the
        /// primary thread. Call ExtractMeshes to build and retrieve the meshes.
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <param name="grid">Grid specifying walls and floors. Must have length and width at most 200.</param>
        public void Generate(WallGrid grid, CaveType type, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            ValidateGrid(grid);
            switch (type)
            {
                case CaveType.Isometric:
                    GenerateIsometric(grid, floorHeightMap, ceilingHeightMap);
                    break;
                case CaveType.Enclosed:
                    GenerateEnclosed(grid, floorHeightMap, ceilingHeightMap);
                    break;
                default:
                    throw new System.ArgumentException("Unrecognized Cave Type.");
            }
        }

        void GenerateIsometric(WallGrid grid, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            ceilingMesh = MeshBuilder.BuildCeiling(grid, ceilingHeightMap);
            outlines    = OutlineGenerator.Generate(ceilingMesh);
            wallMesh    = MeshBuilder.BuildWalls(outlines, floorHeightMap, ceilingHeightMap);
            floorMesh   = MeshBuilder.BuildFloor(grid, floorHeightMap);
        }

        void GenerateEnclosed(WallGrid grid, IHeightMap floorHeightMap, IHeightMap enclosureHeightMap)
        {
            floorMesh   = MeshBuilder.BuildFloor(grid, floorHeightMap);
            outlines    = OutlineGenerator.Generate(floorMesh, reverseOutlines: true);
            ceilingMesh = MeshBuilder.BuildEnclosure(floorMesh, enclosureHeightMap);
            wallMesh    = MeshBuilder.BuildWalls(outlines, floorHeightMap, enclosureHeightMap);
            PruneWallsAtGlobalSeams(grid.Scale);
        }

        public CaveMeshes ExtractMeshes()
        {
            if (floorMesh == null || wallMesh == null || ceilingMesh == null)
                throw new System.InvalidOperationException("Generate meshes before extracting.");

            Mesh floor   = floorMesh.CreateMesh();
            Mesh walls   = wallMesh.CreateMesh();
            Mesh ceiling = ceilingMesh.CreateMesh();

            return new CaveMeshes(floor, walls, ceiling, Index);
        }

        void ValidateGrid(WallGrid grid)
        {
            if (grid == null)
                throw new System.ArgumentNullException("Null grid passed to mesh generator!");

            if (grid.Length > 200 || grid.Width > 200)
                throw new System.ArgumentException("Grid too large for mesh generator. Max size is 200 by 200.");
        }

        // Because of the fact that enclosed caves build walls around floors instead of walls, the floors along the boundary
        // of the map get walls built around them. This method removes those walls. 
        void PruneWallsAtGlobalSeams(int scale)
        {
            Assert.IsNotNull(wallMesh);
            if (DoGlobalSeamsExist()) 
            {
                int modulo = scale * chunkSize.Value;
                WallPruner.PruneModulo(wallMesh, modulo);
            }
        }

        bool DoGlobalSeamsExist()
        {
            return chunkSize.HasValue;
        }
    }
}