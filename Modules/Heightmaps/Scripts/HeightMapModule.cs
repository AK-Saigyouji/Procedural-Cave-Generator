using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration.Modules
{
    public abstract class HeightMapModule : ScriptableObject
    {
        protected const string fileName = "Height Map";
        protected const string rootMenuPath = "Cave Generation/Height Maps/";

        public abstract IHeightMap GetHeightMap();
    } 
}
