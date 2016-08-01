using System;
using System.Collections.Generic;
using UnityEngine;
using CaveGeneration.MeshGeneration;
using CaveGeneration.MapGeneration;

namespace CaveGeneration
{
    public abstract class CaveGenerator : MonoBehaviour
    {
        [SerializeField]
        protected MapParameters mapParameters;

        /// <summary>
        /// Most recently generated cave. Null if no cave has been generated.
        /// </summary>
        public GameObject Cave { get; protected set; }

        /// <summary>
        /// Grid representation of the most recently generated cave. Can be used to figure out where the empty spaces
        /// are in order to procedurally generate content. Do note that the geometry of the cave does not lend itself 
        /// to an exact grid representation, so this is only an approximation.
        /// </summary>
        public Map Map { get; private set; }

        /// <summary>
        /// The generated meshes themselves.
        /// </summary>
        public IList<MapMeshes> GeneratedMeshes { get; protected set; }

        /// <summary>
        /// Property holding the core map parameters such as length, width, density etc. Use this to customize
        /// properties through code.
        /// </summary>
        public MapParameters MapParameters { get { return mapParameters; } protected set { } }

        /// <summary>
        /// Main method for creating cave objects. By default, the cave will be a child of the object holding the cave 
        /// generate script. Can also be accessed through the Cave property. Associated Map object can be accessed
        /// through Map property.
        /// </summary>
        public void GenerateCave()
        {
            DestroyChildren();
            IMapGenerator mapGenerator = GetMapGenerator();
            Map = mapGenerator.GenerateMap();
            GenerateCaveFromMap(Map);
        }

        virtual protected IMapGenerator GetMapGenerator()
        {
            return new MapGenerator(mapParameters);
        }

        /// <summary>
        /// Produces the actual game object from the map as a child of the current game object. 
        /// </summary>
        virtual protected void GenerateCaveFromMap(Map map)
        {
            Cave = CreateChild("Cave", transform);
            IList<Map> submaps = map.Subdivide();
            PrepareHeightMaps();
            MeshGenerator[] meshGenerators = PrepareMeshGenerators(submaps);
            GeneratedMeshes = GenerateMeshes(submaps, meshGenerators);
        }
        
        IList<MapMeshes> GenerateMeshes(IList<Map> submaps, IList<MeshGenerator> meshGenerators)
        {
            List<MapMeshes> meshes = new List<MapMeshes>();
            for (int i = 0; i < submaps.Count; i++)
            {
                meshes.Add(CreateMapMeshes(meshGenerators[i], submaps[i].index));
            }
            return meshes.AsReadOnly();
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

        abstract protected MapMeshes CreateMapMeshes(MeshGenerator meshGenerator, int index);

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
            return CreateChild(name: "Sector " + sectorIndex, parent: Cave.transform);
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

        void Reset()
        {
            mapParameters = new MapParameters();
        }

        void OnValidate()
        {
            mapParameters.OnValidate();
        }
    } 
}