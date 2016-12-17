/* This simple object serves as a package for the result of the cave generator, containing the generated cave itself,
 information about how it was configured, and any additional utility objects tied to the instance itself.*/

using UnityEngine;
using UnityEngine.Assertions;
using CaveGeneration.MapGeneration;
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
        public GameObject      GameObject      { get; private set; }

        /// <summary>
        /// Contains logic for testing the cave's boundaries, allowing objects to be placed dynamically. 
        /// </summary>
        public CollisionTester CollisionTester { get; private set; }

        /// <summary>
        /// A readonly copy of the map configuration used to generate this cave. 
        /// </summary>
        public MapParameters   MapParameters   { get; private set; }

        Sector[] sectors;

        internal Cave(Map map, IEnumerable<CaveMeshes> caveMeshes, MapParameters mapParameters)
        {
            GameObject = new GameObject("Cave");
            CollisionTester = GetCollisionTester(map);
            MapParameters = mapParameters;
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

        CollisionTester GetCollisionTester(Map map)
        {
            return new CollisionTester(new FloorTester(MapConverter.ToWallGrid(map)));
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