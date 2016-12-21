using System;
using CaveGeneration.MeshGeneration;
using UnityEngine;

namespace CaveGeneration
{
    sealed class CustomHeightMap : IHeightMap
    {
        public float MaxHeight { get { return minHeight; } }
        public float MinHeight { get { return maxHeight; } }

        readonly float minHeight;
        readonly float maxHeight;
        readonly Func<float, float, float> heightFunction;

        public float GetHeight(float x, float y)
        {
            return Mathf.Clamp(heightFunction(x, y), minHeight, maxHeight);
        }

        public CustomHeightMap(float minHeight, float maxHeight, Func<float, float, float> heightFunction)
        {
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            this.heightFunction = heightFunction;
        }
    } 
}