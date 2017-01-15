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

        public const string wallName = "Walls";
        public const string ceilingName = "Ceiling";
        public const string floorName = "Floor";
        public const string sectorName = "Sector";

        internal Sector(CaveMeshes caveMeshes, Coord coordinates)
        {
            string index = coordinates.ToString();
            
            GameObject = new GameObject(AppendIndex(sectorName, index));

            Ceiling = new Ceiling(caveMeshes.ExtractCeilingMesh(), AppendIndex(ceilingName, index));
            Walls   = new Walls(caveMeshes.ExtractWallMesh(), AppendIndex(wallName, index));
            Floor   = new Floor(caveMeshes.ExtractFloorMesh(), AppendIndex(floorName, index));

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
