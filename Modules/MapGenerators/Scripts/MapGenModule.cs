using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    /// <summary>
    /// Abstract base class for map generator components (scriptable objects). Implement this to define a custom map 
    /// generator. 
    /// </summary>
    public abstract class MapGenModule : ScriptableObject
    {
        protected const string fileName = "Map Generator";
        protected const string rootMenupath = "Cave Generation/Map Generators/";

        public abstract Map Generate();
    } 
}
