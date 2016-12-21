using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using Stopwatch = System.Diagnostics.Stopwatch;
using CaveGeneration.Utility;
#endif

namespace CaveGeneration
{
    public class CaveGenerator : MonoBehaviour
    {
        /// <summary>
        /// Is the cave generator currently generating a cave? Attempting to extract or generate a cave while a cave is 
        /// being generated will have no effect. 
        /// </summary>
        public bool IsGenerating { get; private set; }

        /// <summary>
        /// Contains all the configurable properties of the cave generator. 
        /// </summary>
        public CaveConfiguration Configuration
        {
            get
            {
                if (!IsGenerating)
                {
                    return configuration;
                }
                else
                {
                    throw new InvalidOperationException("Cannot access configuration during generation.");
                }
            }
        }

        [SerializeField] CaveConfiguration configuration;

        // Unity's coroutines do not support return values, so we save them as instance variables instead.
        // .NET tasks do not have this limitation, but Unity's version of .NET does not have them yet (maybe in 5.6)
        MeshGenerator[] meshGenerators;
        CollisionTester collisionTester;

        Cave Cave;

        /// <summary>
        /// Main method for creating cave objects. Call ExtractCave to get a reference to the most recently generated cave.
        /// If ExtractCave is not called, next call to Generate will destroy the most recently generated cave. Note
        /// that this method is asynchronous so control will be returned before the cave is created. Supply a callback
        /// to consume results as soon as generator is finished.
        /// </summary>
        /// <param name="callback">Optional function to call when generation is finished.</param>
        public void Generate(Action callback = null)
        {
            if (IsGenerating)
            {
                EditorOnlyLog("A cave is already being generated, must finish before another can begin.");
                return;
            }
            DestroyCurrentCave();
            StartCoroutine(GenerateCaveAsync(callback));
        }

        /// <summary>
        /// Gets the most recently generated cave, if it has not already been extracted. Attempting to 
        /// extract during generation or when no cave has been generated will throw an exception. This method
        /// is intended to be called in a callback supplied to the generate method.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public Cave ExtractCave()
        {
            if (IsGenerating || Cave == null)
            {
                string errorMessage = IsGenerating ? "Cannot extract cave while generating." : "No cave to Extract";
                throw new InvalidOperationException(errorMessage);
            }
            Cave extractedCave = Cave;
            Cave = null;
            return extractedCave;
        }

        IEnumerator GenerateCaveAsync(Action callback)
        {
            Setup();
            yield return ExecuteTask(GenerateCoreData);
            yield return BuildCave();
            yield return ActivateChildren();
            TearDown();
            if (callback != null) callback();
        }

        // This method packages the functionality that can be executed in a secondary thread, 
        // which includes generating most of the data necessary to build a cave. BuildCave uses the generated data to 
        // actually build the cave, something that has to be excuted on the main thread.
        void GenerateCoreData()
        {
            MapParameters prevParameters = configuration.MapParameters.Clone();
            Map map = MapGenerator.GenerateMap(prevParameters);
            Map[] submaps = MapSplitter.Subdivide(map);
            MeshGenerator[] meshGenerators = PrepareMeshGenerators(submaps);

            this.collisionTester = MapConverter.ToCollisionTester(map, configuration.Scale);
            this.meshGenerators = meshGenerators;
        }

        IEnumerator BuildCave()
        {
            var caveMeshes = new List<CaveMeshes>();
            foreach (MeshGenerator meshGenerator in meshGenerators)
            {
                caveMeshes.Add(meshGenerator.ExtractMeshes());
                yield return null;
            }
            Cave cave = new Cave(collisionTester, caveMeshes, configuration);
            AssignMaterials(cave.GetFloors(), configuration.FloorMaterial);
            AssignMaterials(cave.GetCeilings(), configuration.CeilingMaterial);
            AssignMaterials(cave.GetWalls(), configuration.WallMaterial);
            Cave = cave;
        }

        MeshGenerator PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            WallGrid wallGrid = MapConverter.ToWallGrid(map, configuration.Scale);
            meshGenerator.Generate(wallGrid, configuration.CaveType, configuration.FloorHeightMap, configuration.CeilingHeightMap);
            return meshGenerator;
        }

        /// <summary>
        /// Creates a mesh generator for each submap and populates the data in each generator necessary to produce meshes.
        /// </summary>
        MeshGenerator[] PrepareMeshGenerators(Map[] submaps)
        {
            var meshGenerators = new MeshGenerator[submaps.Length];
            var actions = new Action[meshGenerators.Length];
            for (int i = 0; i < meshGenerators.Length; i++)
            {
                Map currentMap = submaps[i];
                MeshGenerator meshGenerator = InitializeMeshGenerator(currentMap);
                int indexCopy = i; // using i directly would result in each action using the same value of i
                actions[i] = (() => meshGenerators[indexCopy] = PrepareMeshGenerator(meshGenerator, currentMap));
            }
            if (configuration.DebugMode)
            {
                Array.ForEach(actions, action => action.Invoke());
            }
            else
            {
                Utility.Threading.ParallelExecute(actions);
            }
            return meshGenerators;
        }

        MeshGenerator InitializeMeshGenerator(Map map)
        {
            return new MeshGenerator(MapSplitter.CHUNK_SIZE, map.Index.ToString());
        }

        void AssignMaterials(IEnumerable<CaveComponent> components, Material material)
        {
            foreach (CaveComponent component in components)
            {
                component.Material = material;
            }
        }

        // Sectors are disabled during generation to avoid engaging the Physx engine. They're re-enabled
        // at the end here. In the future this logic could be altered to activate only some of the sectors.
        // e.g. only the sector in which the player starts, leaving the rest disabled until the player gets close to them.
        IEnumerator ActivateChildren()
        {
            EditorOnlyLog("Generation complete, activating sectors...");
            foreach (Sector sector in Cave.GetSectors())
            {
                sector.GameObject.SetActive(true);
                yield return null;
            }
        }

        void Setup()
        {
            IsGenerating = true;
            EditorOnlyLog("Generating cave...");
        }

        void TearDown()
        {
            EditorOnlyLog("Finished!");
            IsGenerating = false;
        }

        IEnumerator ExecuteTask(Action action)
        {
            if (configuration.DebugMode)
            {
                action();
            }
            else
            {
                yield return Utility.Threading.ExecuteAndAwait(action);
            }
        }

        void DestroyCurrentCave()
        {
            if (Cave != null)
            {
                Destroy(Cave.GameObject);
            }
            Cave = null;
        }

        void Reset()
        {
            configuration = new CaveConfiguration();
        }

        void OnValidate()
        {
            configuration.OnValidate();
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