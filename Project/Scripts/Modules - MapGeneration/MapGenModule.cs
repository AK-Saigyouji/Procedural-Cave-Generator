using UnityEngine;
using AKSaigyouji.Maps;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AKSaigyouji.Modules.MapGeneration
{
    /// <summary>
    /// Base class for map generator modules. Implement this to define a custom map generator. 
    /// </summary>
    public abstract class MapGenModule : Module, IBoundedMap
    {
        protected const string fileName = "MapGenerator";
        protected const string rootMenupath = MODULE_ASSET_PATH + "Map Generators/";

        public abstract Map Generate();

        /// <summary>
        /// (length, width) for the map generated according to the current configuration.
        /// </summary>
        public abstract Coord GetMapSize();

        public virtual IEnumerable<Coord> GetBoundary()
        {
            Coord lengthWidth = GetMapSize();
            var boundary = new Boundary(lengthWidth.x, lengthWidth.y);
            return boundary.GetAllCoords();
        }

        public virtual IEnumerable<MapEntrance> GetOpenings()
        {
            return Enumerable.Empty<MapEntrance>();
        }

        public Map Generate(int seed)
        {
            Seed = seed;
            return Generate();
        }
    } 
}
