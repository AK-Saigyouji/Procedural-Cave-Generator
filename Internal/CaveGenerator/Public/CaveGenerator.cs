/* This abstract class is the main driver for the entire cave generation algorithm. It accepts the parameters and delegates
 * responsibility to the appropriate subsystems, in particular the map generator and mesh generator. When implementing
 * a cave generator, it is necessary to override the methods responsible for interfacing with the mesh generator. */

using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;
using CaveGeneration.HeightMaps;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using Stopwatch = System.Diagnostics.Stopwatch;
using CaveGeneration.Utility;
#endif

namespace CaveGeneration
{
    public abstract class CaveGenerator : MonoBehaviour
    {
        [SerializeField] MapParameters mapParameters;

        [Tooltip(Tooltips.CAVE_GEN_DEBUG_MODE)]
        [SerializeField] bool debugMode;

        List<CaveMeshes> generatedMeshes;
        MeshGenerator[] meshGenerators;

        protected GameObject Cave { get; private set; }
        protected IHeightMap FloorHeightMap { get; private set; }
        protected IHeightMap MainHeightMap { get; private set; }

        /// <summary>
        /// Is the cave generator currently generating a cave? Attempting to extract or generate a cave while a cave is 
        /// being generated will have no effect. 
        /// </summary>
        public bool IsGenerating { get; private set; }

        /// <summary>
        /// Readonly grid representation of the most recently generated cave. Can be used to figure out where the empty spaces
        /// are in order to procedurally generate content. Do note that the curved geometry of the cave does not lend itself 
        /// to an exact grid representation, so this is only an approximation.
        /// </summary>
        public Grid Grid { get; private set; }

        /// <summary>
        /// The meshes produced by the cave generator. By default it is not necessary to work directly with these meshes.
        /// </summary>
        public IList<CaveMeshes> GeneratedMeshes
        {
            get { return generatedMeshes != null ? generatedMeshes.AsReadOnly() : null; }
        }

        /// <summary>
        /// Holds the core map parameters such as length, width, density etc. Use this to customize map
        /// properties through code.
        /// </summary>
        public MapParameters MapParameters { get { return mapParameters; } }

        /// <summary>
        /// Main method for creating cave objects. Call ExtractCave to get a reference to the most recently generated cave.
        /// If ExtractCave is not called, next call to Generate will destroy the most recently generated cave.
        /// </summary>
        public void Generate()
        {
            if (IsGenerating)
            {
                EditorOnlyLog("A cave is already being generated, must finish before another can begin.");
                return;
            }
            DestroyCurrentCave();
            PrepareHeightMaps();
            StartCoroutine(GenerateCaveAsync());
        }

        /// <summary>
        /// Gets the most recently generated cave. Will also prevent it from being destroyed by the next call to generate cave.
        /// </summary>
        /// <returns>Most recently generated cave. Null if no cave has been generated or if it's already been extracted.</returns>
        public GameObject ExtractCave()
        {
            if (IsGenerating)
            {
                EditorOnlyLog("Cannot extract cave while it's being generated!");
                return null;
            }
            GameObject temp = Cave;
            Cave = null;
            return temp;
        }

        IEnumerator GenerateCaveAsync()
        {
            Setup();
            if (debugMode)
            {
                GenerateCoreData();
            }
            else
            {
                yield return Utility.Threading.ExecuteAndAwait(GenerateCoreData);
            }
            yield return BuildCave();
            yield return ActivateChildren();
            TearDown();
        }

        /// <summary>
        /// Generate all the data in the MeshGenerator in preparation for the creation of meshes. This method may
        /// get executed outside of the main thread, so don't touch the Unity API when implementing.
        /// </summary>
        abstract protected MeshGenerator PrepareMeshGenerator(MeshGenerator meshGenerator, Map map);

        /// <summary>
        /// Extracts meshes from prepared mesh generator, and builds them into game objects. 
        /// </summary>
        abstract protected CaveMeshes CreateMapMeshes(MeshGenerator meshGenerator, Transform sector);

        void GenerateCoreData()
        {
            IMapGenerator mapGenerator = new MapGenerator(mapParameters);
            Map map = mapGenerator.GenerateMap();
            IList<Map> submaps = MapSplitter.Subdivide(map);
            var PrepareMeshGenerators = SelectMethodForMeshGeneratorPreparation();
            meshGenerators = PrepareMeshGenerators(submaps);
            Grid = new Grid(map);
        }

        IEnumerator BuildCave()
        {
            if (meshGenerators == null) throw new InvalidOperationException("GenerateCoreData must be called before BuildCave.");
            Cave = ObjectFactory.CreateChild(transform, "Cave");
            yield return GenerateMeshes(meshGenerators);
        }

        Func<IList<Map>, MeshGenerator[]> SelectMethodForMeshGeneratorPreparation()
        {
            if (debugMode)
            {
                return PrepareMeshGeneratorsSinglethreaded;
            }
            else
            {
                return PrepareMeshGeneratorsMultithreaded;
            }
        }

        // Sectors are disabled during generation to avoid engaging the Physx engine. They're re-enabled
        // at the end here. In the future this logic could be altered to activate only some of the sectors.
        // e.g. only the sector in which the player starts, leaving the rest disabled until the player gets close to them.
        IEnumerator ActivateChildren()
        {
            EditorOnlyLog("Generation complete, activating sectors...");
            foreach (Transform child in Cave.transform)
            {
                child.gameObject.SetActive(true);
                yield return null;
            }
        }

        void PrepareHeightMaps()
        {
            FloorHeightMap = GetHeightMap<HeightMapFloor>();
            MainHeightMap = GetHeightMap<HeightMapMain>(mapParameters.WallHeight);
        }

        void Setup()
        {
            IsGenerating = true;
            EditorOnlyLog("Generating cave...");
        }

        void TearDown()
        {
            meshGenerators = null;
            EditorOnlyLog("Finished!");
            IsGenerating = false;
        }

        IEnumerator GenerateMeshes(IList<MeshGenerator> meshGenerators)
        {
            generatedMeshes = new List<CaveMeshes>();
            foreach (var meshGenerator in meshGenerators)
            {
                GameObject sector = ObjectFactory.CreateSector(meshGenerator.Index, parent: Cave.transform, active: false);
                generatedMeshes.Add(CreateMapMeshes(meshGenerator, sector.transform));
                yield return null;
            }
        }

        /// <summary>
        /// Creates a mesh generator for each submap and populates the data in each generator necessary to produce meshes.
        /// </summary>
        MeshGenerator[] PrepareMeshGeneratorsMultithreaded(IList<Map> submaps)
        {
            MeshGenerator[] meshGenerators = new MeshGenerator[submaps.Count];
            Action[] actions = new Action[meshGenerators.Length];
            for (int i = 0; i < meshGenerators.Length; i++)
            {
                Map currentMap = submaps[i];
                MeshGenerator meshGenerator = InitializeMeshGenerator(currentMap);
                int indexCopy = i; // using i directly would result in each action using the same value of i
                actions[i] = (() => meshGenerators[indexCopy] = PrepareMeshGenerator(meshGenerator, currentMap));
            }
            Utility.Threading.ParallelExecute(actions);
            return meshGenerators;
        }

        /// <summary>
        /// Singlethreaded version of PrepareMeshGenerators. Useful primarily for debugging and profiling. 
        /// </summary>
        MeshGenerator[] PrepareMeshGeneratorsSinglethreaded(IList<Map> submaps)
        {
            MeshGenerator[] meshGenerators = new MeshGenerator[submaps.Count];
            for (int i = 0; i < meshGenerators.Length; i++)
            {
                Map currentMap = submaps[i];
                MeshGenerator meshGenerator = InitializeMeshGenerator(currentMap);
                meshGenerators[i] = PrepareMeshGenerator(meshGenerator, currentMap);
            }
            return meshGenerators;
        }

        MeshGenerator InitializeMeshGenerator(Map map)
        {
            return new MeshGenerator(MapSplitter.CHUNK_SIZE, map.Index.ToString());
        }

        /// <summary>
        /// Finds the height map builder of type T on this object and extracts a height map from it, or otherwise
        /// returns a constant height map if no builder is found.
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
        }

        void OnValidate()
        {
            mapParameters.OnValidate();
        }

        /// <summary>
        /// This uses Debug.Log to print a message to the Unity console, but only if running in the editor, thus 
        /// avoiding the significant performance hit in a built project.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        void EditorOnlyLog(string message)
        {
            Debug.Log(message);
        }
    } 
}