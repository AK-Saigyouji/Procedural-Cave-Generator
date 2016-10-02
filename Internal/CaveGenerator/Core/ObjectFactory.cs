/* This static class contains methods for creating and configuring game objects. Object creation logic was cluttering up
 the already heavy CaveGenerator class, so the functionality was isolated here.*/

using UnityEngine;

namespace CaveGeneration
{
    public static class ObjectFactory
    {
        /// <summary>
        /// Build and configure a new object as a child of an existing sector/object. Note that the shadowcastingmode 
        /// of the mesh will be set to false.
        /// </summary>
        /// <param name="sector">The new object will be a child of this transform.</param>
        /// <param name="material">The material for the passed in mesh.</param>
        /// <param name="componentName">The name of the component being created, e.g. "Ceiling". Used for naming purposes.</param>
        /// <param name="addCollider">If yes, will build a mesh collider using the passed in mesh.</param>
        /// <returns>The configured mesh used by the new object.</returns>
        public static Mesh CreateComponent(Mesh mesh, Transform sector, Material material, string componentName, bool addCollider)
        {
            mesh.name = GetComponentName(componentName, sector.name);
            GameObject gameObject = CreateGameObjectFromMesh(mesh, componentName, sector, material);
            if (addCollider) AddMeshCollider(gameObject, mesh);
            return mesh;
        }

        /// <summary>
        /// Create a new object with the given parent, labelled with the given index to differentiate it from other
        /// similar objects. Intended use is as a container for the components of a section of a larger object broken
        /// into chunks. 
        /// </summary>
        /// <param name="sectorIndex">Identifier distinguishing this sector from others.</param>
        /// <param name="parent">Parent transform.</param>
        /// <param name="active">Whether the sector should be active.</param>
        public static GameObject CreateSector(string sectorIndex, Transform parent, bool active)
        {
            string name = "Sector " + sectorIndex ?? string.Empty;
            GameObject sector =  CreateChild(parent, name);
            sector.SetActive(active);
            return sector;
        }

        /// <summary>
        /// General helper method for creating an object as the child of another.
        /// </summary>
        public static GameObject CreateChild(Transform parent, string name = "")
        {
            GameObject child = new GameObject(name);
            child.transform.parent = parent;
            return child;
        }

        static string GetComponentName(string component, string index)
        {
            return component + " " + index;
        }

        static void AddMeshCollider(GameObject gameObject, Mesh mesh)
        {
            MeshCollider collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }

        static GameObject CreateGameObjectFromMesh(Mesh mesh, string name, Transform parent, Material material)
        {
            GameObject newObject = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter));
            newObject.transform.parent = parent;
            newObject.GetComponent<MeshFilter>().mesh = mesh;
            newObject.GetComponent<MeshRenderer>().material = material;
            newObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return newObject;
        }
    }
}