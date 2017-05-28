using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.Modules
{
    public sealed class EdgePrefabber : IOutlinePrefabber
    {
        readonly RandomPrefabPicker prefabPicker;
        int rockCounter = 0;

        public EdgePrefabber(RandomPrefabPicker prefabPicker)
        {
            if (prefabPicker == null)
                throw new ArgumentNullException("prefabs");

            this.prefabPicker = prefabPicker;
        }

        public void ProcessOutline(Vector3[] outline, Transform parent)
        {
            // A rock is placed on each edge in the outline, halfway between the endpoints.
            // It is oriented along the direction of the outline.
            int numEdges = outline.Length - 1;
            for (int i = 0; i < numEdges; i++)
            {
                GameObject rockPrefab = prefabPicker.PickRandomPrefab();
                PlaceRockAlongEdge(outline[i], outline[i + 1], rockPrefab, parent);
            }
        }

        void PlaceRockAlongEdge(Vector3 a, Vector3 b, GameObject rockPrefab, Transform parent)
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
            rockInstance.name = string.Format("{0} ({1})", rockPrefab.name, rockCounter);
            rockCounter++;
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