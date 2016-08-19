/* This abstract class is the main driver for the entire cave generation algorithm. It accepts the parameters and delegates
 * responsibility to the appropriate subsystems, in particular the map generator and mesh generator. When implementing
 * a cave generator, it is necessary to override the methods responsible for interfacing with the mesh generator. */

using System;
using System.Collections.Generic;
using UnityEngine;
using CaveGeneration.MeshGeneration;
using CaveGeneration.MapGeneration;
using System.Collections;

namespace CaveGeneration
{
    public abstract class CaveGenerator : MonoBehaviour
    {
        [SerializeField]
        protected MapParameters mapParameters;
        [SerializeField]
        int wallHeight;

        GameObject Cave;
        Map map;
        protected IHeightMap floorHeightMap;
        protected IHeightMap mainHeightMap;

        const int DEFAULT_HEIGHT = 3;
        const int MIN_HEIGHT = 1;

        // This is defined at instance level to work around the inability to return values using Unity coroutines.
        MeshGenerator[] meshGenerators;

        /// <summary>
        /// Is the cave generator currently generating a cave? Attempting to generate or extract a cave while true
        /// will have no effect.
        /// </summary>
        public bool isGenerating { get; private set; }

        /// <summary>
        /// Grid representation of the most recently generated cave. Can be used to figure out where the empty spaces
        /// are in order to procedurally generate content. Do note that the geometry of the cave does not lend itself 
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
        public MapParameters MapParameters { get { return mapParameters; } protected set { } }

        /// <summary>
        /// Height of the walls in the cave, before applying height maps.
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
        public void GenerateCave()
        {
            if (isGenerating)
            {
                Debug.Log("A cave is already being generated, must finish before another can begin.");
                return;
            }
            isGenerating = true;
            Debug.Log("Generating cave...");
            DestroyCurrentCave();
            PrepareHeightMaps();
            StartCoroutine(GenerateCaveAsync());
        }

        IEnumerator GenerateCaveAsync()
        {
            yield return Utility.Threading.ExecuteAndAwait(GenerateMap);
            yield return GenerateCaveFromMap();
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
        /// After meshes have been prepared, this asynchronous method is called to generate MapMeshes objects.
        /// When overriding, add the created MapMesh object to the instance-level MeshGenerators list.
        /// </summary>
        abstract protected IEnumerator CreateMapMeshes(MeshGenerator meshGenerator);

        protected Mesh CreateCeiling(MeshGenerator meshGenerator, Transform sector, Material ceilingMaterial)
        {
            string name = "Ceiling " + meshGenerator.index;
            Mesh ceilingMesh = meshGenerator.GetCeilingMesh();
            ceilingMesh.name = name;
            CreateGameObjectFromMesh(ceilingMesh, name, sector, ceilingMaterial);
            return ceilingMesh;
        }

        protected Mesh CreateWall(MeshGenerator meshGenerator, Transform sector, Material wallMaterial)
        {
            string name = "Wall " + meshGenerator.index;
            Mesh wallMesh = meshGenerator.GetWallMesh();
            wallMesh.name = name;
            GameObject wall = CreateGameObjectFromMesh(wallMesh, name, sector, wallMaterial);
            AddMeshCollider(wall, wallMesh);
            return wallMesh;
        }

        protected Mesh CreateFloor(MeshGenerator meshGenerator, Transform sector, Material floorMaterial)
        {
            string name = "Floor " + meshGenerator.index;
            Mesh floorMesh = meshGenerator.GetFloorMesh();
            floorMesh.name = name;
            GameObject floor = CreateGameObjectFromMesh(floorMesh, name, sector, floorMaterial);
            AddMeshCollider(floor, floorMesh);
            return floorMesh;
        }

        protected Mesh CreateEnclosure(MeshGenerator meshGenerator, Transform sector, Material enclosureMaterial)
        {
            string name = "Enclosure " + meshGenerator.index;
            Mesh enclosureMesh = meshGenerator.GetEnclosureMesh();
            enclosureMesh.name = name;
            CreateGameObjectFromMesh(enclosureMesh, name, sector, enclosureMaterial);
            return enclosureMesh;
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

        // This method gets run on a background thread and constitutes the bulk of the generation.
        // Nothing in this method can touch the Unity API.
        void GenerateMap()
        {
            IMapGenerator mapGenerator = new MapGenerator(mapParameters);
            map = mapGenerator.GenerateMap();
            IList<Map> submaps = map.Subdivide();
            meshGenerators = PrepareMeshGenerators(submaps);
        }

        // This constitutes the rest of the cave generator, the part that must interact with the Unity API.
        IEnumerator GenerateCaveFromMap()
        {
            Cave = CreateChild("Cave", transform);
            yield return GenerateMeshes(meshGenerators);
            Grid = map.ToGrid();
            TearDown();
        }

        void PrepareHeightMaps()
        {
            floorHeightMap = GetHeightMap<HeightMapFloor>();
            mainHeightMap = GetHeightMap<HeightMapMain>(wallHeight);
        }

        void TearDown()
        {
            map = null;
            meshGenerators = null;
            Debug.Log("Cave generated!");
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
        MeshGenerator[] PrepareMeshGenerators(IList<Map> submaps)
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