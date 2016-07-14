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
        public Vector2 ceilingTextureDimensions = new Vector2(100f, 100f);

        Quaternion ORIENTATION_2D = Quaternion.Euler(270f, 0f, 0f);

        protected override GameObject GenerateCaveFromMap(Map map)
        {
            cave = CreateChild("Cave2D", transform);
            IList<Map> submaps = map.Subdivide();
            MeshGenerator[] meshGenerators = PrepareMeshGenerators(submaps);
            List<MapMeshes> meshes = new List<MapMeshes>();
            for (int i = 0; i < submaps.Count; i++)
            {
                GameObject sector = CreateSector(submaps[i].index);
                Mesh mesh = CreateWall(meshGenerators[i], sector);
                meshes.Add(new MapMeshes(ceilingMesh: mesh));
            }
            generatedMeshes = meshes;
            return cave;
        }

        override protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateCeiling(map, ceilingTextureDimensions);
        }

        Mesh CreateWall(MeshGenerator meshGenerator, GameObject parent)
        {
            Mesh ceilingMesh = meshGenerator.GetCeilingMesh();
            GameObject wall = CreateObjectFromMesh(ceilingMesh, "Walls", parent, wallMaterial);
            OrientWall(wall);
            RemoveExistingColliders(wall);
            AddColliders(wall, meshGenerator);
            return ceilingMesh;
        }

        void OrientWall(GameObject wall)
        {
            wall.transform.localRotation = ORIENTATION_2D;
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