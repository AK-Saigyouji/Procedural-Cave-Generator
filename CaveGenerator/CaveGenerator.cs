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

        public GameObject cave { get; protected set; }
        public List<MapMeshes> generatedMeshes { get; protected set; }

        /// <summary>
        /// Main method for creating cave objects. By default, the cave will be a child of the object holding the cave 
        /// generate script. 
        /// </summary>
        public GameObject GenerateCaveUsingInspectorValues()
        {
            MapParameters parameters = new MapParameters(length: length, width: width, mapDensity: initialMapDensity, 
                seed: seed, useRandomSeed: useRandomSeed, squareSize: squareSize, borderSize: borderSize,
                minFloorSize: minFloorSize, minWallSize: minWallSize);

            return GenerateCave(parameters);
        }

        protected GameObject GenerateCave(MapParameters parameters)
        {
            DestroyChildren();
            IMapGenerator mapGenerator = GetMapGenerator(parameters);
            Map map = mapGenerator.GenerateMap();
            return GenerateCaveFromMap(map);
        }

        virtual protected IMapGenerator GetMapGenerator(MapParameters parameters)
        {
            return new MapGenerator(parameters);
        }

        /// <summary>
        /// Produces the actual game object from the map.
        /// </summary>
        /// <returns>A game object holding the final result of the generator.</returns>
        virtual protected GameObject GenerateCaveFromMap(Map map)
        {
            cave = CreateChild("Cave", transform);
            IList<Map> submaps = map.Subdivide();
            PrepareHeightMaps();
            MeshGenerator[] meshGenerators = PrepareMeshGenerators(submaps);
            List<MapMeshes> meshes = new List<MapMeshes>();
            for (int i = 0; i < submaps.Count; i++)
            {
                meshes.Add(CreateMeshes(meshGenerators[i], submaps[i].index));
            }
            generatedMeshes = meshes;
            return cave;
        }
        
        /// <summary>
        /// Any required height maps should be initialized here.
        /// </summary>
        protected virtual void PrepareHeightMaps()
        {

        }

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
        /// Generate all the data in the MeshGenerator in preparation for the creation of meshes. This method may
        /// get executed outside of the main thread, so don't touch the Unity API when implementing.
        /// </summary>
        abstract protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map);

        abstract protected MapMeshes CreateMeshes(MeshGenerator meshGenerator, int index);

        protected GameObject CreateGameObjectFromMesh(Mesh mesh, string name, GameObject parent, Material material, bool castShadows)
        {
            GameObject newObject = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter));
            newObject.transform.parent = parent == null ? null : parent.transform;
            newObject.GetComponent<MeshFilter>().mesh = mesh;
            newObject.GetComponent<MeshRenderer>().material = material;
            SetShadowCastingMode(newObject.GetComponent<MeshRenderer>(), castShadows);
            return newObject;
        }

        void SetShadowCastingMode(MeshRenderer meshRenderer, bool castShadows)
        {
            meshRenderer.shadowCastingMode = 
                castShadows ? UnityEngine.Rendering.ShadowCastingMode.TwoSided : UnityEngine.Rendering.ShadowCastingMode.Off;
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