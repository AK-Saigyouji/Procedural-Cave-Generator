using System;
using UnityEngine;
using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    /// <summary>
    /// Represents a chunk of the cave. Contains references to a floor, ceiling and walls.
    /// </summary>
    public sealed class Sector
    {
        public GameObject GameObject { get; private set; }

        public CaveComponent Ceiling { get; private set; }
        public CaveComponent Walls { get; private set; }
        public CaveComponent Floor { get; private set; }

        const string sectorName = "Sector";
        const string wallName = "Walls";
        const string ceilingName = "Ceiling";
        const string floorName = "Floor";

        internal Sector(CaveMeshes caveMeshes, Coord coordinates)
        {
            string index = coordinates.ToString();
            
            GameObject = new GameObject(AppendIndex(sectorName, index));

            Ceiling = new CaveComponent(caveMeshes.ExtractCeilingMesh(), AppendIndex(ceilingName, index), false);
            Walls   = new CaveComponent(caveMeshes.ExtractWallMesh(), AppendIndex(wallName, index), true);
            Floor   = new CaveComponent(caveMeshes.ExtractFloorMesh(), AppendIndex(floorName, index), true);

            SetChild(Ceiling);
            SetChild(Walls);
            SetChild(Floor);
        }

        public static bool IsFloor(Transform transform)
        {
            if (transform == null)
                return false;

            return transform.name.Contains(floorName);
        }

        public static bool IsWall(Transform transform)
        {
            if (transform == null)
                return false;

            return transform.name.Contains(wallName);
        }

        public static bool IsCeiling(Transform transform)
        {
            if (transform == null)
                return false;

            return transform.name.Contains(ceilingName);
        }

        void SetChild(CaveComponent component)
        {
            component.GameObject.transform.parent = GameObject.transform;
        }

        string AppendIndex(string name, string index)
        {
            return string.Format("{0} {1}", name, index);
        }
    } 
}
