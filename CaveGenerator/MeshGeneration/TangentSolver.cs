/* This class is used to determine tangents for a mesh. Unity has a built in method to compute normals, but nothing
 * for tangents. This class fills that void. 
 */

using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    public static class TangentSolver
    {
        // Passing in the Mesh itself would be ideal here, but accessing data from the mesh
        // requires making copies of each array, which produces massive garbage. MeshData does not
        // create copies.
        public static Vector4[] DetermineTangents(MeshData mesh, Vector3[] normals)
        {
            Vector3[] vertices = mesh.vertices;
            Vector2[] uv = mesh.uv;
            int[] triangles = mesh.triangles;
            int vertexCount = vertices.Length;

            Vector4[] tangents = new Vector4[vertexCount];
            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            for (int tri = 0; tri < triangles.Length; tri += 3)
            {
                int a = triangles[tri];
                int b = triangles[tri + 1];
                int c = triangles[tri + 2];

                Vector3 v1 = vertices[a];
                Vector3 v2 = vertices[b];
                Vector3 v3 = vertices[c];

                Vector2 w1 = uv[a];
                Vector2 w2 = uv[b];
                Vector2 w3 = uv[c];

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

                float r = 1.0f / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[a] += sdir;
                tan1[b] += sdir;
                tan1[c] += sdir;

                tan2[a] += tdir;
                tan2[b] += tdir;
                tan2[c] += tdir;
            }

            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 n = normals[i];
                Vector3 t = tan1[i];
                Vector3.OrthoNormalize(ref n, ref t);
                float w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
                tangents[i] = new Vector4(t.x, t.y, t.z, w);
            }
            return tangents;
        }
    } 
}