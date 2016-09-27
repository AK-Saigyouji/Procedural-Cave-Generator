/* This class is used to determine tangents for a mesh. Unity has a built in method to compute normals, but nothing
 * for tangents. This class fills that void. 
 */

using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    static class TangentSolver
    {
        // It might seem more sensible to just pass in a Mesh to this class, but explicitly passing in the parameters
        // like this has two advantages. One is that extracting data from a Mesh requires making copies, leading to GC 
        // allocations. The other is that using the Mesh class would make this method unsafe to use on secondary threads.

        /// <summary>
        /// Get an array of tangent vectors for each vertex in the mesh, based on its vertices, triangles, normals
        /// and uvs.
        /// </summary>
        public static Vector4[] DetermineTangents(Vector3[] vertices, Vector2[] uv, int[] triangles, Vector3[] normals)
        {
            int vertexCount = vertices.Length;

            Vector4[] tangents = new Vector4[vertexCount];
            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            for (int tri = 0; tri < triangles.Length; tri += 3)
            {
                /* A lot of this code could be written more compactly by using Vector3s and separating functionality
                 * into methods, but the extra copying involved was slower by a large factor.
                 * A few comments have been left above unpacked code to show a more readable (but slower) version 
                 * that should make it easier to follow the logic.
                 */
                int a = triangles[tri];
                int b = triangles[tri + 1];
                int c = triangles[tri + 2];

                Vector3 v1 = vertices[a];
                Vector3 v2 = vertices[b];
                Vector3 v3 = vertices[c];

                Vector2 w1 = uv[a];
                Vector2 w2 = uv[b];
                Vector2 w3 = uv[c];

                //Vector3 lhs = v2 - v1;
                //Vector3 rhs = v3 - v1;
                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;

                float determinant = s1 * t2 - s2 * t1;
                float r = determinant != 0 ? 1 / determinant : 1;

                // Let A be the 2 by 2 matrix [t2, -t1; -s2, s1] of texture differences
                // Let V be the 2 by 3 matrix [lhs; rhs] of vector differences
                // Let r be as above, 1 / det(A).
                // Then we're interested in r * A * V = [sdir; tdir]

                //Vector3 sdir = (t2 * lhs - t1 * rhs) * r;
                float sx = (t2 * x1 - t1 * x2) * r;
                float sy = (t2 * y1 - t1 * y2) * r;
                float sz = (t2 * z1 - t1 * z2) * r;

                //Vector3 tdir = (s1 * rhs - s2 * lhs) * r;
                float tx = (s1 * x2 - s2 * x1) * r;
                float ty = (s1 * y2 - s2 * y1) * r;
                float tz = (s1 * z2 - s2 * z1) * r;

                //tan1[a] += sdir;
                //tan1[b] += sdir;
                //tan1[c] += sdir;

                tan1[a].x += sx;
                tan1[a].y += sy;
                tan1[a].z += sz;
                tan1[b].x += sx;
                tan1[b].y += sy;
                tan1[b].z += sz;
                tan1[c].x += sx;
                tan1[c].y += sy;
                tan1[c].z += sz;

                //tan2[a] += tdir;
                //tan2[b] += tdir;
                //tan2[c] += tdir;

                tan2[a].x += tx;
                tan2[a].y += ty;
                tan2[a].z += tz;
                tan2[b].x += tx;
                tan2[b].y += ty;
                tan2[b].z += tz;
                tan2[c].x += tx;
                tan2[c].y += ty;
                tan2[c].z += tz;
            }

            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 n = normals[i];
                Vector3 t = tan1[i];
                Vector3.OrthoNormalize(ref n, ref t);
                float w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0f) ? -1f : 1f;
                tangents[i] = new Vector4(t.x, t.y, t.z, w);
            }
            return tangents;
        }
    }
}