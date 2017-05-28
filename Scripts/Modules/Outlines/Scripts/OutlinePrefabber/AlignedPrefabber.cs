using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.Modules
{
    public sealed class AlignedPrefabber : IOutlinePrefabber
    {
        readonly System.Random random;
        readonly GameObject[] prefabs;
        readonly int[] risingWeights;
        readonly int maxWeight;
        int rockCounter = 0;

        public AlignedPrefabber(int seed, WeightedPrefab[] prefabs)
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

        public void ProcessOutline(Vector3[] outline, Transform parent)
        {
            // A rock is placed on each edge in the outline, halfway between the endpoints.
            // It is oriented along the direction of the outline.
            int numEdges = outline.Length - 1;
            for (int i = 0; i < numEdges; i++)
            {
                GameObject rockPrefab = PickRandomPrefab();
                GameObject rockInstance = PlaceRockAlongEdge(outline[i], outline[i + 1], rockPrefab, parent);
                rockInstance.name = string.Format("{0} ({1})", rockPrefab.name, rockCounter);
                rockCounter++;
            }
        }

        GameObject PlaceRockAlongEdge(Vector3 a, Vector3 b, GameObject rockPrefab, Transform parent)
        {
            Vector3 position = ComputeMidpoint(a, b);
            Vector3 direction = GetDirection(a, b);
            Quaternion prefabRotation = rockPrefab.transform.rotation;
            GameObject rockInstance = GameObject.Instantiate(rockPrefab, position, prefabRotation, parent);
            if (!IsParallelToTarget(direction))
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, direction);
                rockInstance.transform.rotation = rotation * prefabRotation;
            }
            return rockInstance;
        }

        GameObject PickRandomPrefab()
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

        static Vector3 ComputeMidpoint(Vector3 a, Vector3 b)
        {
            return (a + b) / 2;
        }

        static Vector3 GetDirection(Vector3 a, Vector3 b)
        {
            return (b - a).normalized;
        }

        static bool IsParallelToTarget(Vector3 direction)
        {
            return direction == Vector3.forward || direction == Vector3.back;
        }
    }
}