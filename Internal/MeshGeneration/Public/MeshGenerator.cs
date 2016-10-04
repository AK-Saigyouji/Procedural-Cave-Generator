/* High level class overseeing the mesh generation system. The generate methods are all written in a thread-safe way, 
 * which is why a custom MeshData class is used instead of Unity's unsafe Mesh class. The Create methods are not thread-safe,
 * as they turn the MeshData classes into Meshes. Responsibility for generating individual meshes is delegated to specific
 * MeshBuilders.
 * 
 * The core algorithm driving mesh generation is Marching Squares, which is implemented in the MapTriangulator class. 
 * It turns a grid of 0s (floors) and 1s (walls) into a collection of triangles representing the walls, but with more 
 * structure than simply putting a square at every 1. 
 * 
 * At the moment this class supports Enclosed and Isometric cave types. Isometric caves are designed with an isometric perspective
 * in mind (hence the name) and thus have ceilings that are built over the walls, not the floors. Enclosed are completely 
 * closed off caves designed with a 1st person perspective.
 * 
 * For isometric cave generation, the 1s are triangulated into a ceiling,
 * the 0s are triangulated into a floor. Outlines of the ceiling are computed and quads are built to connect the ceiling
 * and floor meshes, giving complete 3D geometry. Optional height maps then give the geometry added variation by translating
 * the height of floors and ceilings. 
 * 
 * Enclosed cave generation is similar, but instead of triangulating a ceiling, a copy of the floor is made and inverted.
 * Outlines must be inverted since walls face inward for the enclosed caves, rather than outward for the isometric cave.
 */

using System.Collections.Generic;
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

        /// <summary>
        /// Label passed in during construction.
        /// </summary>
        public string Index { get; set; }

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
        /// chunksize. 
        /// </summary>
        public MeshGenerator() : this(null, null) { }

        /// <summary>
        /// Generate the data necessary to produce meshes for isometric type cave. Generates ceiling, wall and floor meshes.
        /// </summary>
        public void GenerateIsometric(WallGrid map, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            GenerateCeiling(map, ceilingHeightMap);
            ComputeMeshOutlines(ceilingMesh);
            GenerateWallsFromCeiling();
            GenerateFloor(map, floorHeightMap);
        }

        /// <summary>
        /// Generate the data necessary to produce meshes for enclosed cave. Generates floor, wall and enclosure meshes.
        /// </summary>
        public void GenerateEnclosed(WallGrid map, IHeightMap floorHeightMap, IHeightMap enclosureHeightMap)
        {
            GenerateFloor(map, floorHeightMap);
            ComputeMeshOutlines(floorMesh);
            ReverseOutlines(); 
            GenerateEnclosure(enclosureHeightMap);
            GenerateWallsFromEnclosure();
            PruneWallsAtGlobalSeams(map.Scale);
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

        // Because of the fact that enclosed caves build walls around floors instead of walls, the floors along the boundary
        // of the map get walls built around them. This method removes those walls. 
        void PruneWallsAtGlobalSeams(int scale)
        {
            if (chunkSize.HasValue) // Global seams exist
            {
                int modulo = scale * chunkSize.Value;
                WallPruner.PruneModulo(wallMesh, modulo);
            }
        }

        void GenerateCeiling(WallGrid map, IHeightMap ceilingHeightMap)
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

        void GenerateFloor(WallGrid map, IHeightMap heightMap)
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