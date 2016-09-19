/* This abstract class is the main driver for the entire cave generation algorithm. It accepts the parameters and delegates
 * responsibility to the appropriate subsystems, in particular the map generator and mesh generator. When implementing
 * a cave generator, it is necessary to override the methods responsible for interfacing with the mesh generator. */

using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration
{
    public abstract class CaveGenerator : MonoBehaviour
    {
        [SerializeField] MapParameters mapParameters;

        [Tooltip("Height of walls before height maps are applied. Minimum of 1.")]
        [SerializeField] int wallHeight;

        [Tooltip("Disables multithreading for profiling and debugging purposes.")]
        [SerializeField] bool debugMode;

        GameObject Cave;
        Map map;

        protected IHeightMap floorHeightMap { get; private set; }
        protected IHeightMap mainHeightMap { get; private set; }

        const int DEFAULT_HEIGHT = 3;
        const int MIN_HEIGHT = 1;

        // This is defined at instance level to work around the inability to return values using Unity coroutines.
        MeshGenerator[] meshGenerators;

        /// <summary>
        /// Is the cave generator currently generating a cave? Attempting to generate or extract a cave will have no effect 
        /// while true.
        /// </summary>
        public bool isGenerating { get; private set; }

        /// <summary>
        /// Grid representation of the most recently generated cave. Can be used to figure out where the empty spaces
        /// are in order to procedurally generate content. Do note that the curved geometry of the cave does not lend itself 
        /// to an exact grid representation, so this is only an approximation.
        /// </summary>
        public Grid Grid { get; private set; }

        /// <summary>
        /// The meshes produced by the cave generator.
        /// </summary>
        public IList<MapMeshes> GeneratedMeshes { get; protected set; }

        /// <summary>
        /// Holds the core map parameters such as length, width, density etc. Use this to customize map
        /// properties through code.
        /// </summary>
        public MapParameters MapParameters { get { return mapParameters; } }

        /// <summary>
        /// Height of the walls in the cave, before applying height maps. Must be at least 1.
        /// </summary>
        public int WallHeight
        {
            get { return wallHeight; }
            set
            {
                if (value >= 1)
                {
                    wallHeight = value;
                }
                else
                {
                    throw new ArgumentException("Wall height must be at least 1");
                }
            }
        }

        /// <summary>
        /// Main method for creating cave objects. Call ExtractCave to get a reference to the most recently generated cave.
        /// If ExtractCave is not called, next call to GenerateCave will override the most recently generated cave.
        /// </summary>
        public void Generate()
        {
            if (isGenerating)
            {
                Debug.Log("A cave is already being generated, must finish before another can begin.");
                return;
            }
            DestroyCurrentCave();
            PrepareHeightMaps();
            StartCoroutine(GenerateCaveAsync());
        }

        IEnumerator GenerateCaveAsync()
        {
            Setup();
            if (debugMode)
            {
                GenerateMapSinglethreaded();
            }
            else
            {
                yield return Utility.Threading.ExecuteAndAwait(GenerateMapMultithreaded);
            }
            yield return GenerateCaveFromMap();
            TearDown();
        }

        /// <summary>
        /// Gets the most recently generated cave. Will also prevent it from being destroyed by the next call to generate cave.
        /// </summary>
        /// <returns>Most recently generated cave. Null if no cave has been generated or if it's already been extracted.</returns>
        public GameObject ExtractCave()
        {
            if (isGenerating)
            {
                Debug.Log("Cannot extract cave while it's being generated!");
                return null;
            }
            GameObject temp = Cave;
            Cave = null;
            return temp;
        }

        /// <summary>
        /// Generate all the data in the MeshGenerator in preparation for the creation of meshes. This method may
        /// get executed outside of the main thread, so don't touch the Unity API when implementing.
        /// </summary>
        abstract protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map);

        /// <summary>
        /// After meshes have been prepared, this method is called to generate MapMeshes objects.
        /// When overriding, add the created MapMesh object to the instance-level MeshGenerators list.
        /// </summary>
        abstract protected IEnumerator CreateMapMeshes(MeshGenerator meshGenerator);

        protected Mesh CreateComponent(Mesh mesh, Transform sector, Material material, string component, Coord index, bool hasCollider)
        {
            string name = GetComponentName(component, index);
            mesh.name = name;
            GameObject gameObject = CreateGameObjectFromMesh(mesh, component, sector, material);
            if (hasCollider) AddMeshCollider(gameObject, mesh);
            return mesh;
        }

        protected GameObject CreateSector(Coord sectorIndex)
        {
            return CreateChild(name: "Sector " + sectorIndex, parent: Cave.transform);
        }

        protected GameObject CreateChild(string name, Transform parent)
        {
            GameObject child = new GameObject(name);
            child.transform.parent = parent;
            return child;
        }

        string GetComponentName(string component, Coord index)
        {
            return component + " " + index;
        }

        void GenerateMapMultithreaded()
        {
            IMapGenerator mapGenerator = new MapGenerator(mapParameters);
            map = mapGenerator.GenerateMap();
            IList<Map> submaps = map.Subdivide();
            meshGenerators = PrepareMeshGeneratorsMultithreaded(submaps);
        }

        void GenerateMapSinglethreaded()
        {
            IMapGenerator mapGenerator = new MapGenerator(mapParameters);
            map = mapGenerator.GenerateMap();
            IList<Map> submaps = map.Subdivide();
            meshGenerators = PrepareMeshGeneratorsSinglethreaded(submaps);
        }

        IEnumerator GenerateCaveFromMap()
        {
            Cave = CreateChild("Cave", transform);
            yield return GenerateMeshes(meshGenerators);
            Grid = map.ToGrid();
        }

        void PrepareHeightMaps()
        {
            floorHeightMap = GetHeightMap<HeightMapFloor>();
            mainHeightMap = GetHeightMap<HeightMapMain>(wallHeight);
        }

        void Setup()
        {
            isGenerating = true;
            Debug.Log("Generating cave...");
        }

        void TearDown()
        {
            map = null;
            meshGenerators = null;
            Debug.Log("Finished!");
            isGenerating = false;
        }

        IEnumerator GenerateMeshes(IList<MeshGenerator> meshGenerators)
        {
            GeneratedMeshes = new List<MapMeshes>();
            foreach (var meshGenerator in meshGenerators)
            {
                yield return CreateMapMeshes(meshGenerator);
            }
        }

        /// <summary>
        /// Creates a mesh generator for each submap and populates the data in each generator necessary to produce meshes.
        /// </summary>
        MeshGenerator[] PrepareMeshGeneratorsMultithreaded(IList<Map> submaps)
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

        /// <summary>
        /// Singlethreaded version of PrepareMeshGenerators. Useful for debugging and profiling.
        /// </summary>
        MeshGenerator[] PrepareMeshGeneratorsSinglethreaded(IList<Map> submaps)
        {
            MeshGenerator[] meshGenerators = InitializeMeshGenerators(submaps.Count);
            for (int i = 0; i < meshGenerators.Length; i++)
            {
                PrepareMeshGenerator(meshGenerators[i], submaps[i]);
            }
            return meshGenerators;
        }

        MeshGenerator[] InitializeMeshGenerators(int count)
        {
            MeshGenerator[] meshGenerators = new MeshGenerator[count];
            for (int i = 0; i < count; i++)
            {
                meshGenerators[i] = new MeshGenerator();
            }
            return meshGenerators;
        }

        void AddMeshCollider(GameObject gameObject, Mesh mesh)
        {
            MeshCollider collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }

        GameObject CreateGameObjectFromMesh(Mesh mesh, string name, Transform parent, Material material)
        {
            GameObject newObject = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter));
            newObject.transform.parent = parent;
            newObject.GetComponent<MeshFilter>().mesh = mesh;
            newObject.GetComponent<MeshRenderer>().material = material;
            newObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return newObject;
        }

        /// <summary>
        /// Finds the height map builder of type T on this object and extracts a height map from it, or otherwise
        /// returns a constant height map.
        /// </summary>
        IHeightMap GetHeightMap<T>(int baseHeight = 0) where T : HeightMapBuilder
        {
            HeightMapBuilder heightMapBuilder = GetComponent<T>();
            IHeightMap heightMap;
            if (heightMapBuilder != null)
            {
                int seed = mapParameters.Seed.GetHashCode();
                heightMap = heightMapBuilder.Build(seed, baseHeight);
            }
            else
            {
                heightMap = new ConstantHeightMap(baseHeight);
            }
            return heightMap;
        }

        void DestroyCurrentCave()
        {
            if (Cave != null)
            {
                Destroy(Cave);
            }
        }

        void Reset()
        {
            mapParameters = new MapParameters();
            wallHeight = DEFAULT_HEIGHT;
        }

        void OnValidate()
        {
            mapParameters.OnValidate();
            if (wallHeight < MIN_HEIGHT)
            {
                wallHeight = MIN_HEIGHT;
            }
        }
    } 
}