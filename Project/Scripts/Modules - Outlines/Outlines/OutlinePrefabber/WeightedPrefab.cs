using System;
using UnityEngine;

namespace AKSaigyouji.Modules.Outlines
{
    [Serializable]
    public sealed class WeightedPrefab
    {
        [SerializeField] GameObject prefab;
        [SerializeField] int weight;

        public GameObject Prefab { get { return prefab; } }
        public int Weight { get { return weight; } }

        const int MIN_WEIGHT = 1;

        /// <summary>
        /// Represents a prefab with a numerical weight.
        /// </summary>
        /// <param name="weight">Must be at least 1.</param>
        public WeightedPrefab(GameObject prefab, int weight)
        {
            if (prefab == null)
                throw new ArgumentNullException("prefab");

            if (weight < MIN_WEIGHT)
                throw new ArgumentOutOfRangeException("weight");

            this.prefab = prefab;
            this.weight = weight;
        }

        public void OnValidate()
        {
            weight = Mathf.Max(MIN_WEIGHT, weight);
        }
    } 
}