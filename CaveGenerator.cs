using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using Stopwatch = System.Diagnostics.Stopwatch;
using CaveGeneration.Utility;
using UnityEngine.Profiling;
#endif

namespace CaveGeneration
{
    public class CaveGenerator : MonoBehaviour
    {
        /// <summary>
        /// Is the cave generator currently generating a cave? Attempting to extract or generate a cave while a cave is 
        /// being generated will result in an InvalidOperationException.
        /// </summary>
        public bool IsGenerating { get; private set; }

        /// <summary>
        /// Contains all the configurable properties of the cave generator. 
        /// </summary>
        public CaveConfiguration Configuration
        {
            get { return config; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                else
                {
                    config = value.Clone();
                }
            }
        }

        // Caution: any change to this field's name needs to be reflected in the custom inspector script
        // for the latter to work properly.
        [SerializeField] CaveConfiguration config = new CaveConfiguration();

        IHeightMap ceilingHeightMap;
        IHeightMap floorHeightMap;

        // Unity's coroutines do not support return values, so we save them as instance variables instead.
        // .NET tasks do not have this limitation, but Unity's version of .NET does not have them yet
        CaveMeshChunk[] caveChunks;
        CollisionTester collisionTester;

        Cave cave;

        /// <summary>
        /// Main method for creating cave objects. Call ExtractCave to get a reference to the most recently generated cave.
        /// If ExtractCave is not called, next call to Generate will destroy the most recently generated cave. Note
        /// that this method is asynchronous so control will be returned before the cave is created. Supply a callback
        /// to consume results as soon as generator is finished. If a given heightmap is left null, the inspector
        /// values will be used to generate the corresponding heightmap. 
        /// </summary>
        /// <param name="callback">Optional function to call when generation is finished.</param>
        /// <param name="floorHeightMap">Height map for the floor. If null, will use inspector values to build.</param>
        /// <param name="ceilingHeightMap">Height map for the ceiling. If null, will use inspector values to build.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Generate(Action callback = null, IHeightMap floorHeightMap = null, IHeightMap ceilingHeightMap = null)
        {
            if (IsGenerating)
                throw new InvalidOperationException("Cave is already being generated.");

            this.floorHeightMap = floorHeightMap ?? config.FloorHeightMap;
            this.ceilingHeightMap = ceilingHeightMap ?? config.CeilingHeightMap;
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
            if (IsGenerating || cave == null)
            {
                string errorMessage = IsGenerating ? "Cannot extract cave while generating." : "No cave to Extract";
                throw new InvalidOperationException(errorMessage);
            }
            Cave extractedCave = cave;
            cave = null;
            return extractedCave;
        }

        IEnumerator GenerateCaveAsync(Action callback)
        {
            IsGenerating = true;
            EditorOnlyLog("Generating cave...");

            yield return ExecuteTask(GenerateCoreData);
            this.cave = new Cave(collisionTester, caveChunks, config);

            EditorOnlyLog("Finished!");
            IsGenerating = false;

            if (callback != null) callback();
        }

        // This method packages the functionality that can be executed in a secondary thread, 
        // which includes generating most of the data necessary to build a cave. 
        void GenerateCoreData()
        {
            Map map = MapGenerator.GenerateMap(config.MapParameters);
            CaveMeshChunk[] caveChunks = GenerateMeshes(map);
            CollisionTester collisionTester = MapConverter.ToCollisionTester(map, config.Scale);

            this.collisionTester = collisionTester;
            this.caveChunks = caveChunks;
        }

        CaveMeshChunk[] GenerateMeshes(Map map)
        {
            MapChunk[] mapChunks = MapSplitter.Subdivide(map);
            var caveChunks = new CaveMeshChunk[mapChunks.Length];
            var actions = new Action[mapChunks.Length];
            for (int i = 0; i < mapChunks.Length; i++)
            {
                int indexCopy = i; // using i directly would result in each action using the same value of i
                actions[i] = (() => caveChunks[indexCopy] = GenerateMeshes(mapChunks[indexCopy]));
            }
            ExecuteActions(actions);
            return caveChunks;
        }

        CaveMeshChunk GenerateMeshes(MapChunk mapChunk)
        {
            WallGrid wallGrid = MapConverter.ToWallGrid(mapChunk, config.Scale);
            CaveMeshes meshes = MeshGenerator.Generate(wallGrid, config.CaveType, floorHeightMap, ceilingHeightMap);
            return new CaveMeshChunk(meshes, mapChunk.Index);
        }

        void ExecuteActions(Action[] actions)
        {
            if (config.DebugMode)
            {
                Array.ForEach(actions, action => action.Invoke());
            }
            else
            {
                Utility.Threading.ParallelExecute(actions);
            }
        }

        IEnumerator ExecuteTask(Action action)
        {
            if (config.DebugMode)
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
            if (cave != null)
            {
                Destroy(cave.GameObject);
            }
            cave = null;
        }

        void Reset()
        {
            config = new CaveConfiguration();
        }

        void OnValidate()
        {
            config.OnValidate();
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