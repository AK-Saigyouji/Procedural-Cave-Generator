using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.Modules
{
    /// <summary>
    /// Instantiates prefabs along the outlines of a cave.
    /// </summary>
    public interface IOutlinePrefabber
    {
        void ProcessOutline(Vector3[] outline, Transform parent);
    } 
}