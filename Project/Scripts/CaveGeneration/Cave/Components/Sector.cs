using UnityEngine;
using AKSaigyouji.MeshGeneration;

namespace AKSaigyouji.CaveGeneration
{
    /// <summary>
    /// Represents a chunk of the cave. Contains references to the ceiling, wall and floor components.
    /// </summary>
    public sealed class Sector
    {
        public GameObject GameObject { get; private set; }

        public CaveComponent Ceiling { get; private set; }
        public CaveComponent Walls { get; private set; }
        public CaveComponent Floor { get; private set; }

        const string SECTOR_NAME = "Sector";
        const string WALL_NAME = "Walls";
        const string CEILING_NAME = "Ceiling";
        const string FLOOR_NAME = "Floor";

        internal Sector(CaveMeshes caveMeshes, int x, int y)
        {
            string index = string.Format("{0},{1}", x, y);
            
            GameObject = new GameObject(AppendIndex(SECTOR_NAME, index));

            if (caveMeshes.HasCeilingMesh)
            {
                Ceiling = new CaveComponent(caveMeshes.ExtractCeilingMesh(), AppendIndex(CEILING_NAME, index), addCollider: false);
                SetChild(Ceiling);
            }
            if (caveMeshes.HasFloorMesh)
            {
                Floor = new CaveComponent(caveMeshes.ExtractFloorMesh(), AppendIndex(FLOOR_NAME, index), true);
                SetChild(Floor);
            }
            if (caveMeshes.HasWallMesh)
            {
                Walls = new CaveComponent(caveMeshes.ExtractWallMesh(), AppendIndex(WALL_NAME, index), true);
                SetChild(Walls);
            }
        }

        public static bool IsFloor(Transform transform)
        {
            if (transform == null)
                return false;

            return transform.name.Contains(FLOOR_NAME);
        }

        public static bool IsWall(Transform transform)
        {
            if (transform == null)
                return false;

            return transform.name.Contains(WALL_NAME);
        }

        public static bool IsCeiling(Transform transform)
        {
            if (transform == null)
                return false;

            return transform.name.Contains(CEILING_NAME);
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
