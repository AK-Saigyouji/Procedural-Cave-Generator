using System;
using System.Linq;
using UnityEngine;

namespace AKSaigyouji.Modules.Outlines
{
    public sealed class WeightedPrefabPicker: IPrefabPicker
    {
        readonly System.Random random;
        readonly GameObject[] prefabs;
        readonly int[] risingWeights;
        readonly int maxWeight;

        public WeightedPrefabPicker(WeightedPrefab[] prefabs, int seed)
        {
            if (prefabs == null)
                throw new ArgumentNullException("prefabs");

            var validPrefabs = prefabs.Where(prefab => prefab.Prefab != null);

            if (!validPrefabs.Any())
                throw new ArgumentException("No valid prefabs assigned to outline prefabber.");

            this.prefabs = validPrefabs.Select(prefab => prefab.Prefab).ToArray();
            int[] rawWeights = validPrefabs.Select(prefab => prefab.Weight).ToArray();
            int[] risingWeights = new int[this.prefabs.Length];
            for (int i = 0, totalWeight = 0; i < rawWeights.Length; i++)
            {
                totalWeight += rawWeights[i];
                risingWeights[i] = totalWeight;
            }
            maxWeight = risingWeights[risingWeights.Length - 1];
            this.risingWeights = risingWeights;
            random = new System.Random(seed);
        }

        public GameObject PickPrefab()
        {
            int randomWeight = random.Next(0, maxWeight + 1);
            for (int i = 0; i < risingWeights.Length; i++)
            {
                if (risingWeights[i] >= randomWeight) // final weight in rising weights guaranteed to satisfy this.
                {
                    return prefabs[i];
                }
            }
            throw new InvalidOperationException("Internal error in prefabber - random prefab selection failed.");
        }
    } 
}