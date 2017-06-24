/* This object serves as a package for the result of the three tier cave generator.*/

using AKSaigyouji.ArrayExtensions;
using AKSaigyouji.MeshGeneration;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AKSaigyouji.CaveGeneration
{
    [Serializable]
    public sealed class ThreeTierCave
    {
        /// <summary>
        /// The root game object for the cave. 
        /// </summary>
        public GameObject GameObject { get; private set; }

        Sector[] sectors;

        internal ThreeTierCave(CaveMeshes[,] caveMeshes)
        {
            Assert.IsNotNull(caveMeshes);

            GameObject = new GameObject("Cave");

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

        void BuildSectors(CaveMeshes[,] caveChunks)
        {
            var sectors = new List<Sector>();
            caveChunks.ForEach((x, y) =>
            {
                var sector = new Sector(caveChunks[x, y], x, y);
                sector.GameObject.transform.parent = GameObject.transform;
                sectors.Add(sector);
            });
            this.sectors = sectors.ToArray();
        }
    }
}