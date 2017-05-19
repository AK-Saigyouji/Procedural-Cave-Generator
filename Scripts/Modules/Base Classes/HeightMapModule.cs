using UnityEngine;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration.Modules
{
    public abstract class HeightMapModule : Module
    {
        protected const string fileName = "Height Map";
        protected const string rootMenuPath = "Cave Generation/Height Maps/";

        public abstract IHeightMap GetHeightMap();
    } 
}
