using System;
using System.Linq;
using UnityEngine;

namespace AKSaigyouji.Modules.Outlines
{
    public sealed class EdgePrefabber
    {
        readonly IPrefabPicker prefabPicker;
        int rockCounter = 0;

        public EdgePrefabber(IPrefabPicker prefabPicker)
        {
            if (prefabPicker == null)
                throw new ArgumentNullException("prefabs");

            this.prefabPicker = prefabPicker;
        }

        public void ProcessOutline(Outline outline, Transform parent)
        {
            foreach (Edge edge in outline.GetEdges())
            {
                GameObject rockPrefab = prefabPicker.PickPrefab();
                Vector3 position = edge.MidPoint;
                Vector3 direction = edge.Direction;
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
        }

        static bool IsParallelToTarget(Vector3 direction)
        {
            return direction == Vector3.forward || direction == Vector3.back;
        }
    }
}