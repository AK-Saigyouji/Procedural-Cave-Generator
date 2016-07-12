using System;
using System.Collections.Generic;
using UnityEngine;
using CaveGeneration.MeshGeneration;
using CaveGeneration.MapGeneration;

namespace CaveGeneration
{
    public abstract class CaveGenerator : MonoBehaviour
    {
        public int length = 50;
        public int width = 50;
        [Range(0.4f, 0.6f)]
        public float initialMapDensity = 0.5f;
        public string seed;
        public bool useRandomSeed = true;
        public int borderSize = 0;
        public int squareSize = 1;
        public int minWallSize = 50;
        public int minFloorSize = 50;
        public Vector2 ceilingTextureDimensions = new Vector2(100f, 100f);

        public GameObject cave { get; protected set; }
        public List<MapMeshes> generatedMeshes { get; protected set; }

        public void GenerateCaveUsingInspectorValues()
        {
            MapParameters parameters = new MapParameters(length: length, width: width, mapDensity: initialMapDensity, 
                seed: seed, useRandomSeed: useRandomSeed, squareSize: squareSize, borderSize: borderSize,
                minFloorSize: minFloorSize, minWallSize: minWallSize);

            GenerateCave(parameters);
        }

        protected void GenerateCave(MapParameters parameters)
        {
            DestroyChildren();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            IMapGenerator mapGenerator = GetMapGenerator(parameters);
            Map map = mapGenerator.GenerateMap();
            Utility.Stopwatch.Query(sw, "Map: ");
            GenerateMeshFromMap(map);
            Utility.Stopwatch.Query(sw, "Mesh: ");
        }

        virtual protected IMapGenerator GetMapGenerator(MapParameters parameters)
        {
            return new MapGenerator(parameters);
        }

        abstract protected void GenerateMeshFromMap(Map map);

        /// <summary>
        /// Creates a mesh generator for each submap and populates the data in each generator necessary to produce meshes.
        /// </summary>
        protected MeshGenerator[] PrepareMeshGenerators(IList<Map> submaps)
        {
            MeshGenerator[] meshGenerators = InitializeMeshGenerators(submaps.Count);
            Action[] actions = new Action[meshGenerators.Length];
            for (int i = 0; i < meshGenerators.Length; i++)
            {
                int indexCopy = i;
                actions[i] = (() => PrepareMeshGenerator(meshGenerators[indexCopy], submaps[indexCopy]));
            }
            Utility.Threading.ParallelExecute(actions);
            return meshGenerators;
        }

        protected MeshGenerator[] InitializeMeshGenerators(int count)
        {
            MeshGenerator[] meshGenerators = new MeshGenerator[count];
            for (int i = 0; i < count; i++)
            {
                meshGenerators[i] = new MeshGenerator();
            }
            return meshGenerators;
        }

        /// <summary>
        /// Generate all the data in the MeshGenerator in preparation for the creation of meshes. Each call
        /// of this method will get distributed across threads, so override with care: work done in this method
        /// must not touch the Unity API and any shared data must be accessed in a threadsafe way.
        /// </summary>
        virtual protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateCeiling(map, ceilingTextureDimensions);
        }

        protected GameObject CreateObjectFromMesh(Mesh mesh, string name, GameObject parent, Material material)
        {
            GameObject newObject = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter));
            newObject.transform.parent = parent == null ? null : parent.transform;
            newObject.GetComponent<MeshFilter>().mesh = mesh;
            newObject.GetComponent<MeshRenderer>().material = material;
            return newObject;
        }

        protected GameObject CreateSector(int sectorIndex)
        {
            return CreateChild(name: "Sector " + sectorIndex, parent: cave.transform);
        }

        protected GameObject CreateChild(string name, Transform parent)
        {
            GameObject child = new GameObject(name);
            child.transform.parent = parent;
            return child;
        }

        void DestroyChildren()
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in transform)
            {
                children.Add(child);
            }
            foreach (Transform child in children)
            {
                child.parent = null;
                Destroy(child.gameObject);
            }
        }
    } 
}