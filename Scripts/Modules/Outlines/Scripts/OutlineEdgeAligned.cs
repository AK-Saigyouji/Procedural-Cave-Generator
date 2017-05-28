using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + "Edge-Aligned")]
    public sealed class OutlineEdgeAligned : OutlineModule
    {
        [SerializeField] WeightedPrefab[] rockPrefabs;
        [SerializeField] int seed;

        public void SetRockPrefabs(params GameObject[] prefabs)
        {
            if (prefabs == null)
                throw new ArgumentNullException("prefabs");

            if (prefabs.Length == 0)
                throw new ArgumentException("Must assign at least one rock prefab.");

            prefabs = prefabs.Where(r => r != null).ToArray();
        }

        public IEnumerable<WeightedPrefab> GetRockPrefabs()
        {
            return rockPrefabs;
        }

        public override int Seed { get { return seed; } set { seed = value; } }

        public override IOutlinePrefabber GetOutlinePrefabber()
        {
            if (rockPrefabs == null)
                throw new InvalidOperationException("Rock prefabs not set.");

            if (rockPrefabs.Length == 0)
                throw new InvalidOperationException("Must assign at least one rock prefab.");

            var prefabPicker = new RandomPrefabPicker(rockPrefabs, seed);
            return new EdgePrefabber(prefabPicker);
        }

        void OnValidate()
        {
            foreach (WeightedPrefab wPrefab in rockPrefabs)
            {
                wPrefab.OnValidate();
            }
        }
    }
}