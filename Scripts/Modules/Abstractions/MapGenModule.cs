using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    /// <summary>
    /// Base class for map generator modules. Implement this to define a custom map generator. 
    /// </summary>
    public abstract class MapGenModule : Module
    {
        protected const string fileName = "MapGenerator";
        protected const string rootMenupath = "Cave Generation/Map Generators/";

        public abstract Map Generate();

        public Map Generate(int seed)
        {
            Seed = seed;
            return Generate();
        }
    } 
}
