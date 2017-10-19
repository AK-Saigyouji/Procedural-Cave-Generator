/* This corresponds to the cave walls that were generated before cave wall modules were implemented: a simple quad is 
 * built between a pair of adjacent outline vertices and the corresponding ceiling vertices. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.Modules.CaveWalls
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + "Flat (Default)")]
    sealed class CaveWallFlat : CaveWallModule
    {
        public override int ExtraVerticesPerCorner { get { return 0; } }

        public override Vector3 GetAdjustedCorner(VertexContext context)
        {
            return context.Vertex;
        }
    }
}