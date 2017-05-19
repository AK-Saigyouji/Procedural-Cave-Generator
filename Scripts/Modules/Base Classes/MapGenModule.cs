using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    /// <summary>
    /// Base class for map generator modules. Implement this to define a custom map generator. 
    /// </summary>
    public abstract class MapGenModule : Module
    {
        protected const string fileName = "Map Generator";
        protected const string rootMenupath = "Cave Generation/Map Generators/";

        public abstract Map Generate();
    } 
}
