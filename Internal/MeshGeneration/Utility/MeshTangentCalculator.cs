/* This class is used to determine tangents for a mesh. Unity has a built in method to compute normals, but nothing
 * for tangents. This class fills that void by providing an extension method RecalculateTangents similar to the built-in
 * instance method RecalculateNormals. Additional functionality is provided for use in performance sensitive environments
 * in which heavy GC allocations are undesirable. For most use cases, it is sufficient to use the parameterless 
 * extension method. 
 * 
 * Nomenclature: tangent space is normally given by three vectors forming an orthonormal basis. Tangent, binormal/bitangent, 
 * and normal. In the context of a Unity mesh, a tangent is a Vector4 (x,y,z,w) where (x,y,z) is the tangent, and 
 * w is either 1 or -1, corresponding to whether the crossproduct of the tangent and normal needs to be flipped to 
 * recover the bitangent. In this script, tangents refer to the Vector4, tan refers to the tangent space tangent (a vector3),
 * and bitan refers to the bitangent (another vector3). 
 * 
 * Note that the tangents produced are not the same as the ones produced internally by Unity, as it handles triangles
 * along seams differently. In this script, distinct vertices in the vertices array with the same position are treated 
 * as completely independent. Subtle differences can be observed, for example, by comparing a bumped texture 
 * on Unity's primitive meshes before and after using this script to recalculate tangents. 
 */

using UnityEngine;
using System.Collections.Generic;

namespace MeshTangentCalculator
{
    public static class MeshTangentCalculator
    {
        /// <summary>
        /// The approximate amount of memory, in bytes, being occupied by the buffers created by UseBuffers. Use
        /// DestroyBuffers to clear this memory. Always 0 if not using buffers. 
        /// </summary>
        public static int BufferMemory
        {
            get
            {
                return usingBuffers ? (tanBuffer.Length + biTanBuffer.Length) * 12 + tangentsBuffer.Capacity * 16 : 0;
            }
        }

        static Vector3[] tanBuffer;
        static Vector3[] biTanBuffer;
        static List<Vector4> tangentsBuffer;

        static bool usingBuffers = false; // Are we re-using the above buffers or allocating new memory every time?
        const int MAX_BUFFER_SIZE = ushort.MaxValue; // Meshes cannot store more than this number of vertices.

        const string nullArgumentString = "Arguments must not be null!";

        /// <summary>
        /// Recalculates the tangents of the mesh from the vertices, triangles, texture coordinates (uv) and normals. 
        /// Performance note: this method requires copying a large amount of data from the mesh, resulting in a lot of 
        /// GC allocations.
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void RecalculateTangents(this Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] uv = mesh.uv;
            int[] triangles = mesh.triangles;
            mesh.RecalculateTangents(vertices, triangles, uv, normals);
        }

        /// <summary>
        /// Recalculate the tangents of the mesh using the supplied vertices, triangles, texture coordinates and normals.
        /// Avoids copying data from the mesh, dramatically reducing GC allocations. Useful if this data is already available
        /// from building a mesh manually through code.
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void RecalculateTangents(this Mesh mesh, Vector3[] vertices, int[] triangles, Vector2[] uv, Vector3[] normals)
        {
            ValidateInput(vertices, triangles, uv, normals);

            int numVertices = vertices.Length;
            Vector3[] tan = GetTanArray(numVertices);
            Vector3[] biTan = GetBiTanArray(numVertices);

            int numTriangles = triangles.Length / 3;
            for (int i = 0; i < numTriangles; i++)
            {
                int triangleIndex = 3 * i;
                int a = triangles[triangleIndex];
                int b = triangles[triangleIndex + 1];
                int c = triangles[triangleIndex + 2];

                Vector3 v1 = vertices[a];
                Vector3 v2 = vertices[b];
                Vector3 v3 = vertices[c];

                Vector2 w1 = uv[a];
                Vector2 w2 = uv[b];
                Vector2 w3 = uv[c];

                float x1 = v2.x - v1.x;
                float y1 = v2.y - v1.y;
                float z1 = v2.z - v1.z;

                float x2 = v3.x - v1.x;
                float y2 = v3.y - v1.y;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float t1 = w2.y - w1.y;

                float s2 = w3.x - w1.x;
                float t2 = w3.y - w1.y;

                float determinant = s1 * t2 - s2 * t1;
                float detInverse = determinant != 0 ? 1 / determinant : 0f;

                s1 *= detInverse;
                t1 *= detInverse;
                s2 *= detInverse;
                t2 *= detInverse;

                float tx = (t2 * x1 - t1 * x2);
                float ty = (t2 * y1 - t1 * y2);
                float tz = (t2 * z1 - t1 * z2);

                float bx = (s1 * x2 - s2 * x1);
                float by = (s1 * y2 - s2 * y1);
                float bz = (s1 * z2 - s2 * z1);

                tan[a].x += tx; tan[b].x += tx; tan[c].x += tx;
                tan[a].y += ty; tan[b].y += ty; tan[c].y += ty;
                tan[a].z += tz; tan[b].z += tz; tan[c].z += tz;

                biTan[a].x += bx; biTan[b].x += bx; biTan[c].x += bx;
                biTan[a].y += by; biTan[b].y += by; biTan[c].y += by;
                biTan[a].z += bz; biTan[b].z += bz; biTan[c].z += bz;
            }

            List<Vector4> tangents = GetTangentsList(numVertices);
            for (int i = 0; i < normals.Length; i++)
            {
                Vector3 n = normals[i];
                Vector3 t = tan[i];
                Vector3 b = biTan[i];
                Vector3.OrthoNormalize(ref n, ref t, ref b);
                tangents.Add(new Vector4(t.x, t.y, t.z,
                    // This is Vector3.Dot(Vector3.Cross(n, t), b) but much faster
                    (b.x * (n.y * t.z - n.z * t.y) + b.y * (n.z * t.x - n.x * t.z) + b.z * (n.x * t.y - n.y * t.x)) 
                    > 0f ? 1f : -1f)
                );
            }
            mesh.SetTangents(tangents);

            ClearBuffers();
        }

        /// <summary>
        /// Recalculate the tangents of the mesh using the supplied vertices, triangles, texture coordinates and normals as lists.
        /// Useful if frequently rebuilding dynamic meshes in a performance-sensitive environment. Also see UseBuffers, which 
        /// combined with this overload can allow for dynamically rebuilding meshes without allocations.
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void RecalculateTangents(this Mesh mesh, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, List<Vector3> normals)
        {
            ValidateInput(vertices, triangles, uv, normals);

            int numVertices = vertices.Count;
            Vector3[] tan = GetTanArray(numVertices);
            Vector3[] biTan = GetBiTanArray(numVertices);

            int numTriangles = triangles.Count / 3;
            for (int i = 0; i < numTriangles; i++)
            {
                int triangleIndex = 3 * i;
                int a = triangles[triangleIndex];
                int b = triangles[triangleIndex + 1];
                int c = triangles[triangleIndex + 2];

                Vector3 v1 = vertices[a];
                Vector3 v2 = vertices[b];
                Vector3 v3 = vertices[c];

                Vector2 w1 = uv[a];
                Vector2 w2 = uv[b];
                Vector2 w3 = uv[c];

                float x1 = v2.x - v1.x;
                float y1 = v2.y - v1.y;
                float z1 = v2.z - v1.z;

                float x2 = v3.x - v1.x;
                float y2 = v3.y - v1.y;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float t1 = w2.y - w1.y;

                float s2 = w3.x - w1.x;
                float t2 = w3.y - w1.y;

                float determinant = s1 * t2 - s2 * t1;
                float detInverse = determinant != 0 ? 1 / determinant : 0f;

                s1 *= detInverse;
                t1 *= detInverse;
                s2 *= detInverse;
                t2 *= detInverse;

                float tx = (t2 * x1 - t1 * x2);
                float ty = (t2 * y1 - t1 * y2);
                float tz = (t2 * z1 - t1 * z2);

                float bx = (s1 * x2 - s2 * x1);
                float by = (s1 * y2 - s2 * y1);
                float bz = (s1 * z2 - s2 * z1);

                tan[a].x += tx; tan[b].x += tx; tan[c].x += tx;
                tan[a].y += ty; tan[b].y += ty; tan[c].y += ty;
                tan[a].z += tz; tan[b].z += tz; tan[c].z += tz;

                biTan[a].x += bx; biTan[b].x += bx; biTan[c].x += bx;
                biTan[a].y += by; biTan[b].y += by; biTan[c].y += by;
                biTan[a].z += bz; biTan[b].z += bz; biTan[c].z += bz;
            }

            List<Vector4> tangents = GetTangentsList(numVertices);
            for (int i = 0; i < normals.Count; i++)
            {
                Vector3 n = normals[i];
                Vector3 t = tan[i];
                Vector3 b = biTan[i];
                Vector3.OrthoNormalize(ref n, ref t, ref b);
                tangents.Add(new Vector4(t.x, t.y, t.z,
                    // This is Vector3.Dot(Vector3.Cross(n, t), b) but much faster
                    (b.x * (n.y * t.z - n.z * t.y) + b.y * (n.z * t.x - n.x * t.z) + b.z * (n.x * t.y - n.y * t.x))
                    > 0f ? 1f : -1f)
                );
            }
            mesh.SetTangents(tangents);

            ClearBuffers();
        }

        /// <summary>
        /// Force temporary data structures to be re-used instead of re-allocating them every time a recalculate method
        /// is used. This will substantially reduce GC allocations at the expense of holding onto the memory until 
        /// DestroyBuffers is called. Use the BufferMemory property to check how much memory is currently being used by the 
        /// buffers.
        /// </summary>
        /// <param name="capacity">Max number of vertices that will need to be stored. Default corresponds to the
        /// maximum number of vertices possible in a mesh. Recalculating tangents on a mesh with more vertices than capacity
        /// will result in buffers resizing until max is reached.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static void UseBuffers(int capacity = ushort.MaxValue)
        {
            if (capacity < 0) throw new System.ArgumentOutOfRangeException("Cannot have negative capacity!");
            usingBuffers = true;
            capacity = Mathf.Clamp(capacity, 1, ushort.MaxValue);
            tanBuffer = new Vector3[capacity];
            biTanBuffer = new Vector3[capacity];
            tangentsBuffer = new List<Vector4>(capacity);
        }

        /// <summary>
        /// Clears the buffers created by the UseBuffers method, freeing the memory. Does nothing if UseBuffers has not been
        /// called, or if they've already been destroyed. 
        /// </summary>
        public static void DestroyBuffers()
        {
            tanBuffer = null;
            biTanBuffer = null;
            tangentsBuffer = null;

            usingBuffers = false;
        }

        static Vector3[] GetTanArray(int requiredLength)
        {
            if (usingBuffers)
            {
                if (tanBuffer.Length < requiredLength)
                {
                    tanBuffer = new Vector3[GetNewBufferSize(tanBuffer.Length)];
                }
                return tanBuffer;
            }
            else
            {
                return new Vector3[requiredLength];
            }
        }

        static Vector3[] GetBiTanArray(int requiredLength)
        {
            if (usingBuffers)
            {
                if (biTanBuffer.Length < requiredLength)
                {
                    biTanBuffer = new Vector3[GetNewBufferSize(biTanBuffer.Length)];
                }
                return biTanBuffer;
            }
            else
            {
                return new Vector3[requiredLength];
            }
        }

        static List<Vector4> GetTangentsList(int requiredCapacity)
        {
            return usingBuffers ? tangentsBuffer : new List<Vector4>(requiredCapacity);
        }

        // Doubles the size of buffers, up to the maximum size.
        static int GetNewBufferSize(int currentSize)
        {
            return Mathf.Min(MAX_BUFFER_SIZE, currentSize * 2);
        }

        // Empties buffers without freeing memory.
        static void ClearBuffers()
        {
            if (usingBuffers)
            {
                System.Array.Clear(tanBuffer,   0, tanBuffer.Length);
                System.Array.Clear(biTanBuffer, 0, biTanBuffer.Length);
                tangentsBuffer.Clear();
            }
        }

        static void ValidateInput(IList<Vector3> vertices, IList<int> triangles, IList<Vector2> uv, IList<Vector3> normals)
        {
            if (vertices  == null) throw new System.ArgumentNullException("vertices",  nullArgumentString);
            if (triangles == null) throw new System.ArgumentNullException("triangles", nullArgumentString);
            if (uv        == null) throw new System.ArgumentNullException("uv",        nullArgumentString);
            if (normals   == null) throw new System.ArgumentNullException("normals",   nullArgumentString);

            if (vertices.Count != uv.Count || vertices.Count != normals.Count)
            {
                string format = "Vertices, uvs and normals must have same length. Vertices: {0}. UVs: {1}. Normals: {2}.";
                string errorMessage = string.Format(format, vertices.Count, uv.Count, normals.Count);
                throw new System.ArgumentException(errorMessage);
            }
        }
    }
}