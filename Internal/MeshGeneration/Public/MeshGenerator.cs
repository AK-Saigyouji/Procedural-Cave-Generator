/* High level class overseeing the mesh generation system, written to be usable on multiple threads. This is
 * why a custom MeshData class is used instead of Unity's unsafe Mesh class. 
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
 * Outlines are computed and quads are built to connect the ceiling and floor meshes, giving complete 3D 
 * geometry. Optional height maps then give the geometry added variation by translating the height of floors and ceilings. 
 * 
 * Enclosed cave generation is similar, but instead of triangulating a ceiling, a copy of the floor is made and inverted.
 */

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
        /// Generates meshes for a 3D cave. Safe to call outside the main thread.
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <param name="grid">Grid specifying walls and floors. Must have length and width at most 200.</param>
        /// <param name="type">Enumerated type with options for what type of cave is being generated.</param>
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
                    throw new System.ArgumentException("Unrecognized Cave Type.");
            }
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
            MeshData ceiling = MeshBuilder.BuildEnclosure(floor, enclosureHeightMap);
            MeshData wall    = MeshBuilder.BuildWalls(grid, floorHeightMap, enclosureHeightMap);
            return new CaveMeshes(floor, wall, ceiling);
        }

        static void ValidateInput(WallGrid grid, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            if (grid == null)
                throw new System.ArgumentNullException("grid");

            if (grid.Length > MAX_SIZE || grid.Width > MAX_SIZE)
                throw new System.ArgumentException(string.Format("Max grid size is {0} by {0}", MAX_SIZE));

            if (floorHeightMap == null)
                throw new System.ArgumentNullException("floorHeightMap");

            if (ceilingHeightMap == null)
                throw new System.ArgumentNullException("ceilingHeightMap");
        }
    }
}