﻿/* This abstract class is the main driver for the entire cave generation algorithm. It accepts the parameters and delegates
 * responsibility to the appropriate subsystems, in particular the map generator and mesh generator. When implementing
 * a cave generator, it is necessary to override the methods responsible for interfacing with the mesh generator. */

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

        GameObject Cave;

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
        /// Property holding the core map parameters such as length, width, density etc. Use this to customize
        /// properties through code.
        /// </summary>
        public MapParameters MapParameters { get { return mapParameters; } protected set { } }

        /// <summary>
        /// Main method for creating cave objects. Call ExtractCave to get a reference to the most recently generated cave.
        /// If ExtractCave is not called, next call to GenerateCave will override the most recently generated cave.
        /// </summary>
        public void GenerateCave()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            DestroyCurrentCave();
            IMapGenerator mapGenerator = GetMapGenerator();
            Map Map = mapGenerator.GenerateMap();
            Utility.Stopwatch.Query(sw, "Time:");
            GenerateCaveFromMap(Map);
            Utility.Stopwatch.Query(sw, "Time:");
            Grid = Map.ToGrid();
        }

        /// <summary>
        /// Gets the most recently generated cave. Will also prevent it from being destroyed by the next call to generate cave.
        /// </summary>
        /// <returns>Most recently generated cave. Null if no cave has been generated or if it's already been extracted.</returns>
        public GameObject ExtractCave()
        {
            GameObject temp = Cave;
            Cave = null;
            return temp;
        }

        virtual protected IMapGenerator GetMapGenerator()
        {
            return new MapGenerator(mapParameters);
        }

        /// <summary>
        /// Produce the actual game object from the map as a child of the current game object. 
        /// </summary>
        protected void GenerateCaveFromMap(Map map)
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
        protected virtual void PrepareHeightMaps() { }

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

        /// <summary>
        /// Singlethreaded version of PrepareMeshGenerators. Useful for debugging and profiling.
        /// </summary>
        protected MeshGenerator[] PrepareMeshGeneratorsSinglethreaded(IList<Map> submaps)
        {
            MeshGenerator[] meshGenerators = InitializeMeshGenerators(submaps.Count);
            for (int i = 0; i < meshGenerators.Length; i++)
            {
                PrepareMeshGenerator(meshGenerators[i], submaps[i]);
            }
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

        abstract protected MapMeshes CreateMapMeshes(MeshGenerator meshGenerator, Coord index);

        protected GameObject CreateGameObjectFromMesh(Mesh mesh, string name, GameObject parent, Material material)
        {
            GameObject newObject = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter));
            newObject.transform.parent = parent == null ? null : parent.transform;
            newObject.GetComponent<MeshFilter>().mesh = mesh;
            newObject.GetComponent<MeshRenderer>().material = material;
            newObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return newObject;
        }

        /// <summary>
        /// Finds the height map builder of type T on this object and extracts a height map from it, or otherwise
        /// returns a constant height map.
        /// </summary>
        protected IHeightMap GetHeightMap<T>(int baseHeight) where T : HeightMapBuilder
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
    } 
}