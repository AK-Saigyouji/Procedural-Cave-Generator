using System;
using UnityEngine;

namespace CaveGeneration
{
    public sealed class CaveComponent
    {
        public string Name { get; private set; }

        public GameObject GameObject { get; private set; }

        /// <summary>
        /// The mesh for this component. Shortcut for this object's mesh filter's shared mesh.
        /// </summary>
        public Mesh Mesh { get { return meshFilter.sharedMesh; } }

        /// <summary>
        /// The material for this component. Shortcut for the material property on this object's meshFilter component.
        /// </summary>
        public Material Material
        {
            get { return meshRenderer.material; }
            set { meshRenderer.material = value; }
        }

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;

        internal CaveComponent(Mesh mesh, string name, bool addCollider)
        {
            Name = name;
            mesh.name = name;

            GameObject = BuildGameObject(mesh);

            if (addCollider)
                AddCollider(mesh);
        }

        GameObject BuildGameObject(Mesh mesh)
        {
            var gameObject = new GameObject(Name);
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return gameObject;
        }

        void AddCollider(Mesh mesh)
        {
            MeshCollider collider = GameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }
    } 
}
