using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace CaveGeneration.Modules
{
    [Serializable]
    public sealed class WeightedPrefab
    {
        [SerializeField] GameObject prefab;
        [SerializeField] int weight;

        public GameObject Prefab { get { return prefab; } }
        public int Weight { get { return weight; } }

        /// <summary>
        /// Represents a prefab with a numerical weight.
        /// </summary>
        /// <param name="weight">Must be at least 1.</param>
        public WeightedPrefab(GameObject prefab, int weight)
        {
            if (prefab == null)
                throw new ArgumentNullException("prefab");

            if (weight < 1)
                throw new ArgumentOutOfRangeException("weight");

            this.prefab = prefab;
            this.weight = weight;
        }
    } 
}