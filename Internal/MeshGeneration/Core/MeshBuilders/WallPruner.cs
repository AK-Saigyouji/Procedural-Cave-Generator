/* This class was created to address a serious bug that arose when using a floor mesh to determine outlines.
 * Each mesh is only a chunk that has to be patched together with other chunks. If two side-by-side meshes are connected
 * along floors, then the outline generator would create outlines along that boundary, which would ultimately result
 * in walls being created where they shouldn't be. This class takes a generated wall mesh and removes the triangles
 * corresponding to those walls.
 */

using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Prunes triangles in a mesh.
    /// </summary>
    static class WallPruner
    {
        /// <summary>
        /// Prune triangles if they lie entirely on a horizontal or vertical line whose position is a multiple
        /// of the given modulus. e.g. if mod = 50, then a triangle whose x-coordinates are all 150 will be removed.
        /// </summary>
        public static void PruneModulo(MeshData mesh, int mod)
        {
            int numIndicesToRemove = MarkTrianglesForRemoval(mesh, mod);
            int finalLength = mesh.triangles.Length - numIndicesToRemove;
            mesh.triangles = PruneTriangles(mesh.triangles, finalLength);
        }

        /// <summary>
        /// Triangles to be pruned have their values set to -1. 
        /// </summary>
        /// <returns>Number of triangles to be pruned.</returns>
        static int MarkTrianglesForRemoval(MeshData mesh, int mod)
        {
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;

            int numIndicesToRemove = 0;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = vertices[triangles[i]];
                Vector3 b = vertices[triangles[i + 1]];
                Vector3 c = vertices[triangles[i + 2]];
                if (IsBoundaryTriangle(a, b, c, mod))
                {
                    numIndicesToRemove += 3;
                    triangles[i] = -1;
                    triangles[i + 1] = -1;
                    triangles[i + 2] = -1;
                }
            }
            return numIndicesToRemove;
        }

        static int[] PruneTriangles(int[] triangles, int newLength)
        {
            int[] newTriangles = new int[newLength];
            for (int i = 0, newIndex = 0; i < triangles.Length; i += 3)
            {
                if (triangles[i] > -1)
                {
                    newTriangles[newIndex++] = triangles[i];
                    newTriangles[newIndex++] = triangles[i + 1];
                    newTriangles[newIndex++] = triangles[i + 2];
                }
            }

            return newTriangles;
        }

        static bool IsBoundaryTriangle(Vector3 a, Vector3 b, Vector3 c, int mod)
        {
            float x = (a.x + b.x + c.x) / 3;
            float z = (a.z + b.z + c.z) / 3;
            int boundaryIndex = mod;
            return (x % boundaryIndex == 0) || (z % boundaryIndex == 0);
        }
    } 
}