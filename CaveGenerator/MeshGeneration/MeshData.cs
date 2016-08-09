﻿using System.Collections;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Holds the data necessary to build a mesh. Use in place of the built-in Mesh class when working outside of the primary 
    /// thread since the Unity API is not thread safe. Use Mesh in every other situation. Unlike Mesh, using getters
    /// does not create copies. 
    /// </summary>
    public class MeshData
    {
        public Vector3[] vertices { get; set; }
        public Vector2[] uv { get; set; }
        public int[] triangles { get; set; }
        public string name { get; set; }
    } 
}
