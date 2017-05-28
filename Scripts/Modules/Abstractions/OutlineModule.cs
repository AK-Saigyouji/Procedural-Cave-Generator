using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.Modules
{
    public abstract class OutlineModule : Module
    {
        protected const string fileName = "OutlinePrefabber";
        protected const string rootMenupath = "Cave Generation/Outline Prefabbers/";

        public abstract IOutlinePrefabber GetOutlinePrefabber();
    } 
}