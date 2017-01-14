/* This object serves as a package for the result of the cave generator, containing the generated cave itself,
 information about how it was configured, and any additional utility objects tied to the instance itself.*/

using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration
{
    /// <summary>
    /// Result of the cave generator. The cave is laid out in the hierarchy as follows: at the top sits the Cave
    /// game object. It has 1 or more sectors as children, corresponding to chunks of the cave.
    /// Each sector contains three components: a wall, a floor, and a ceiling.
    /// </summary>
    [Serializable]
    public sealed class Cave
    {
        /// <summary>
        /// The root game object for the cave. 
        /// </summary>
        public GameObject GameObject { get; private set; }

        Sector[] sectors;
        CaveConfiguration configuration;

        internal Cave(IEnumerable<CaveMeshChunk> caveMeshes, CaveConfiguration caveConfig)
        {
            Assert.IsNotNull(caveMeshes);
            Assert.IsNotNull(caveConfig);
            Assert.IsFalse(caveMeshes.Contains(null));

            configuration = caveConfig;
            GameObject = new GameObject("Cave");

            BuildSectors(caveMeshes);

            AssignMaterial(GetWalls(), caveConfig.WallMaterial);
            AssignMaterial(GetFloors(), caveConfig.FloorMaterial);
            AssignMaterial(GetCeilings(), caveConfig.CeilingMaterial);
        }

        /// <summary>
        /// Retrieve a copy of the configuration used to produce this cave. If the components used to construct the
        /// cave produce a deterministic result with respect to its properties, then it's possible to rebuild the 
        /// same cave using this configuration. An example of when this is not possible is when using the default 
        /// map generator component configured to use a random seed.
        /// </summary>
        public CaveConfiguration GetConfiguration()
        {
            return configuration.Clone();
        }

        /// <summary>
        /// Builds a utility object that allows for testing if objects collide with this cave's walls. 
        /// </summary>
        public CollisionTester BuildCollisionTester()
        {
            Map map = configuration.MapGenerator.Generate();
            var tester = MapConverter.ToCollisionTester(map, configuration.Scale);
            return tester;
        }

        public IEnumerable<CaveComponent> GetFloors()
        {
            return sectors.Select(sector => sector.Floor);
        }

        public IEnumerable<CaveComponent> GetWalls()
        {
            return sectors.Select(sector => sector.Walls);
        }

        public IEnumerable<CaveComponent> GetCeilings()
        {
            return sectors.Select(sector => sector.Ceiling);
        }

        /// <summary>
        /// Return a flat sequence of all the components, i.e. floors, walls, and ceilings.
        /// </summary>
        public IEnumerable<CaveComponent> GetAllComponents()
        {
            return GetFloors().Concat(GetWalls()).Concat(GetCeilings());
        }

        public IEnumerable<Sector> GetSectors()
        {
            return sectors;
        }

        void AssignMaterial(IEnumerable<CaveComponent> components, Material material)
        {
            foreach (CaveComponent component in components)
            {
                component.Material = material;
            }
        }

        void BuildSectors(IEnumerable<CaveMeshChunk> caveChunks)
        {
            var sectors = new List<Sector>();
            foreach (CaveMeshChunk caveChunk in caveChunks)
            {
                var sector = new Sector(caveChunk);
                sector.GameObject.transform.parent = GameObject.transform;
                sectors.Add(sector);
            }
            this.sectors = sectors.ToArray();
        }
    }
}