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

        internal Cave(CollisionTester collisionTester, IEnumerable<CaveMeshes> caveMeshes, CaveConfiguration caveConfiguration)
        {
            Assert.IsNotNull(collisionTester);
            Assert.IsNotNull(caveMeshes);
            Assert.IsNotNull(caveConfiguration);

            Configuration = caveConfiguration.Clone();
            GameObject = new GameObject("Cave");
            CollisionTester = collisionTester;
            BuildSectors(caveMeshes);
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

        public IEnumerable<Sector> GetSectors()
        {
            return sectors;
        }

        void BuildSectors(IEnumerable<CaveMeshes> caveMeshes)
        {
            var sectors = new List<Sector>();
            foreach (CaveMeshes caveMesh in caveMeshes)
            {
                var sector = new Sector(caveMesh, caveMesh.Index);
                sector.GameObject.transform.parent = GameObject.transform;
                sectors.Add(sector);
            }
            this.sectors = sectors.ToArray();
        }
    }
}