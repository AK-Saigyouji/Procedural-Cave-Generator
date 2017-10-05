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
    sealed class CaveWallFlat : CaveWallModule
    {
        public CaveWallFlat() : base(MIN_VERTICES_PER_CORNER) { }

        public override Vector3 GetAdjustedCorner(Vector3 original, Vector3 normal, float floorHeight, float ceilingHeight)
        {
            // To get a flat cave wall, we simply return the unaltered corner: 
            return original;
        }
    }
}