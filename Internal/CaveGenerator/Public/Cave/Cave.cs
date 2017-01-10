/* This simple object serves as a package for the result of the cave generator, containing the generated cave itself,
 information about how it was configured, and any additional utility objects tied to the instance itself.*/

using UnityEngine;
using UnityEngine.Assertions;
using CaveGeneration.MeshGeneration;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration
{
    /// <summary>
    /// A cave produced by the CaveGeneration system.
    /// </summary>
    public sealed class Cave
    {
        /// <summary>
        /// The actual cave GameObject itself. 
        /// </summary>
        public GameObject GameObject { get; private set; }

        /// <summary>
        /// Contains logic for testing the cave's boundaries, allowing objects to be placed dynamically. 
        /// </summary>
        public CollisionTester CollisionTester { get; private set; }

        /// <summary>
        /// The configuration used to generate this cave.
        /// </summary>
        public CaveConfiguration Configuration { get; private set; }

        Sector[] sectors;

        internal Cave(CollisionTester collisionTester, IEnumerable<CaveMeshChunk> caveMeshes, CaveConfiguration caveConfiguration)
        {
            Assert.IsNotNull(collisionTester);
            Assert.IsNotNull(caveMeshes);
            Assert.IsNotNull(caveConfiguration);
            Assert.IsFalse(caveMeshes.Contains(null));

            Configuration = caveConfiguration.Clone();
            GameObject = new GameObject("Cave");
            CollisionTester = collisionTester;

            BuildSectors(caveMeshes);

            AssignMaterial(GetWalls(), Configuration.WallMaterial);
            AssignMaterial(GetFloors(), Configuration.FloorMaterial);
            AssignMaterial(GetCeilings(), Configuration.CeilingMaterial);
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