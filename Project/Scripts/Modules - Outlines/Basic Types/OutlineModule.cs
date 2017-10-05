using System.Collections.Generic;
using UnityEngine;

namespace AKSaigyouji.Modules.Outlines
{
    public abstract class OutlineModule : Module
    {
        protected const string fileName = "OutlinePrefabber";
        protected const string outlineMenuPath = MODULE_ASSET_PATH + "Outline Prefabbers/";

        public abstract void ProcessOutlines(IEnumerable<Outline> outlines, Transform parent);
    } 
}