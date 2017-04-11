/* High level class overseeing the mesh generation system. The core algorithm driving mesh generation is Marching Squares. 
 * It turns a grid of 0s (floors) and 1s (walls) into a collection of triangles representing the walls, but with more 
 * structure than simply putting a square at every 1. 
 * 
 * At the moment this class supports Enclosed and Isometric cave types. Isometric caves are designed with an isometric perspective
 * in mind (hence the name) and thus have ceilings that are built over the walls, not the floors. Enclosed caves are completely 
 * closed off caves designed with a 1st person perspective in mind.
 * 
 * For isometric cave generation, the 1s are triangulated into a ceiling, and the 0s are triangulated into a floor. 
 * Outlines are computed and quads are built to connect the ceiling and floor meshes, giving complete 3D 
 * geometry. Optional height maps then give the geometry added variation by translating the height of floors and ceilings. 
 * 
 * Enclosed cave generation is similar, but the ceiling is an inverted copy of the floor: the 1s in the grid are 
 * actually not used at all.
 */

using System;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    public enum CaveType
    {
        Isometric,
        Enclosed
    }

    public static class MeshGenerator
    {
        const int MAX_SIZE = 200;

        /// <summary>
        /// Generates meshes for a 3D cave. Safe to call outside the main thread. Note that the height map
        /// for the floor must have a max height that is at most the min height of the ceiling height map.
        /// </summary>
        /// <param name="grid">Grid specifying walls and floors. Must have length and width at most 200.</param>
        /// <param name="type">Enumerated type with options for what type of cave is being generated.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static CaveMeshes Generate(WallGrid grid, CaveType type, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            ValidateInput(grid, floorHeightMap, ceilingHeightMap);
            switch (type)
            {
                case CaveType.Isometric:
                    return GenerateIsometric(grid, floorHeightMap, ceilingHeightMap);
                case CaveType.Enclosed:
                    return GenerateEnclosed(grid, floorHeightMap, ceilingHeightMap);
                default:
                    throw new ArgumentException("Unrecognized Cave Type.");
            }
        }

        // In some cases, generating the entire cave is wasteful - these methods give the option
        // of just generating individual pieces. They return meshes directly, and thus must be called on main thread.

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static Mesh GenerateFloorMesh(WallGrid grid, IHeightMap floorHeightMap)
        {
            ValidateGrid(grid);

            if (floorHeightMap == null)
                throw new ArgumentNullException("floorHeightMap");

            MeshData floor = MeshBuilder.BuildFloor(grid, floorHeightMap);
            return floor.CreateMesh();
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static Mesh GenerateIsometricCeiling(WallGrid grid, IHeightMap ceilingHeightMap)
        {
            ValidateGrid(grid);

            if (ceilingHeightMap == null)
                throw new ArgumentNullException("ceilingHeightMap");

            MeshData ceiling = MeshBuilder.BuildCeiling(grid, ceilingHeightMap);
            return ceiling.CreateMesh();
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static Mesh GenerateEnclosedCeiling(WallGrid grid, IHeightMap ceilingHeightMap)
        {
            ValidateGrid(grid);

            if (ceilingHeightMap == null)
                throw new ArgumentNullException("ceilingHeightMap");

            MeshData ceiling = MeshBuilder.BuildEnclosure(grid, ceilingHeightMap);
            return ceiling.CreateMesh();
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static Mesh GenerateWallMesh(WallGrid grid, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            ValidateInput(grid, floorHeightMap, ceilingHeightMap);
            MeshData walls = MeshBuilder.BuildWalls(grid, floorHeightMap, ceilingHeightMap);
            return walls.CreateMesh();
        }

        static CaveMeshes GenerateIsometric(WallGrid grid, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            MeshData floor   = MeshBuilder.BuildFloor(grid, floorHeightMap);
            MeshData ceiling = MeshBuilder.BuildCeiling(grid, ceilingHeightMap);
            MeshData wall    = MeshBuilder.BuildWalls(grid, floorHeightMap, ceilingHeightMap);
            return new CaveMeshes(floor, ceiling, wall);
        }

        static CaveMeshes GenerateEnclosed(WallGrid grid, IHeightMap floorHeightMap, IHeightMap enclosureHeightMap)
        {
            MeshData floor   = MeshBuilder.BuildFloor(grid, floorHeightMap);
            MeshData ceiling = MeshBuilder.BuildEnclosure(grid, enclosureHeightMap);
            MeshData wall    = MeshBuilder.BuildWalls(grid, floorHeightMap, enclosureHeightMap);
            return new CaveMeshes(floor, wall, ceiling);
        }

        static void ValidateInput(WallGrid grid, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            ValidateGrid(grid);

            if (floorHeightMap == null)
                throw new ArgumentNullException("floorHeightMap");

            if (ceilingHeightMap == null)
                throw new ArgumentNullException("ceilingHeightMap");

            if (floorHeightMap.MaxHeight > ceilingHeightMap.MinHeight)
                throw new ArgumentException("Height values for the floor cannot be greater than for the ceiling.");
        }

        static void ValidateGrid(WallGrid grid)
        {
            if (grid == null)
                throw new ArgumentNullException("grid");

            if (grid.Length > MAX_SIZE || grid.Width > MAX_SIZE)
                throw new ArgumentException(string.Format("Max grid size is {0} by {0}", MAX_SIZE));
        }
    }
}