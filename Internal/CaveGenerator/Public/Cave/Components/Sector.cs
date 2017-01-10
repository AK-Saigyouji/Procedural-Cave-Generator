using UnityEngine;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    public sealed class Sector
    {
        public GameObject GameObject { get; private set; }

        public CaveComponent Ceiling { get; private set; }
        public CaveComponent Walls { get; private set; }
        public CaveComponent Floor { get; private set; }

        internal Sector(CaveMeshChunk caveChunk)
        {
            string index = caveChunk.Index.ToString();
            CaveMeshes caveMeshes = caveChunk.CaveMeshes;
            
            GameObject = new GameObject(AppendIndex("Sector", index));

            Ceiling = new Ceiling(caveMeshes.ExtractCeilingMesh(), AppendIndex("Ceiling", index));
            Walls   = new Walls(caveMeshes.ExtractWallMesh(), AppendIndex("Walls", index));
            Floor   = new Floor(caveMeshes.ExtractFloorMesh(), AppendIndex("Floor", index));

            SetChild(Ceiling);
            SetChild(Walls);
            SetChild(Floor);
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
