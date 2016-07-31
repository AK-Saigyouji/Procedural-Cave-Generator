using UnityEngine;
using CaveGeneration.MeshGeneration;
using System.Collections.Generic;

namespace CaveGeneration
{
    /// <summary>
    /// A 2D map generator, intended to be used in 2D mode. Generates flat cavernous regions with edge colliders that run along
    /// the outlines of these regions.
    /// </summary>
    public class CaveGenerator2D : CaveGenerator
    {
        public Material wallMaterial;
        public Material floorMaterial;

        readonly Quaternion ORIENTATION_2D = Quaternion.Euler(270f, 0f, 0f);

        override protected MapMeshes CreateMapMeshes(MeshGenerator meshGenerator, int index)
        {
            GameObject sector = CreateSector(index);
            Mesh ceilingMesh = CreateCeiling(meshGenerator, sector);
            Mesh floorMesh = CreateFloor(meshGenerator, sector);
            return new MapMeshes(ceilingMesh, floorMesh);
        }

        override protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateCeiling(map);
            meshGenerator.GenerateFloor(map);
        }

        Mesh CreateCeiling(MeshGenerator meshGenerator, GameObject parent)
        {
            Mesh ceilingMesh = meshGenerator.GetCeilingMesh();
            GameObject wall = CreateGameObjectFromMesh(ceilingMesh, "Walls", parent, wallMaterial, false);
            Orient2D(wall);
            RemoveExistingColliders(wall);
            AddColliders(wall, meshGenerator);
            return ceilingMesh;
        }

        Mesh CreateFloor(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh floorMesh = meshGenerator.GetFloorMesh();
            GameObject floor = CreateGameObjectFromMesh(floorMesh, "Floor", sector, floorMaterial, false);
            Orient2D(floor);
            return floorMesh;
        }

        void Orient2D(GameObject gameObject)
        {
            gameObject.transform.localRotation = ORIENTATION_2D;
        }

        void RemoveExistingColliders(GameObject wall)
        {
            EdgeCollider2D[] currentColliders = wall.GetComponents<EdgeCollider2D>();
            foreach (EdgeCollider2D collider in currentColliders)
            {
                Destroy(collider);
            }
        }

        void AddColliders(GameObject wall, MeshGenerator meshGenerator)
        {
            List<Vector2[]> edgePointsList = meshGenerator.GetOutlines();
            foreach (Vector2[] edgePoints in edgePointsList)
            {
                EdgeCollider2D edgeCollider = wall.AddComponent<EdgeCollider2D>();
                edgeCollider.points = edgePoints;
            }
        }
    }
}