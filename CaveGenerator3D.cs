﻿using UnityEngine;
using System.Collections.Generic;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    /// <summary>
    /// A 3D map generator. Generates flat cavernous regions and perpendicular walls along the outlines of those regions.
    /// The walls receive a mesh collider for collision detection.
    /// </summary>
    public class CaveGenerator3D : CaveGenerator
    {
        public int wallHeight = 3;
        public Material ceilingMaterial;
        public Material wallMaterial;
        public Material floorMaterial;
        public int wallsPerTextureTile = 5;

        override protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateCeiling(map);
            meshGenerator.GenerateWalls(wallsPerTextureTile, wallHeight);
            meshGenerator.GenerateFloor(map);
        }

        protected override MapMeshes CreateMeshes(MeshGenerator meshGenerator, int index)
        {
            GameObject sector = CreateSector(index);
            Mesh ceilingMesh = CreateCeiling(meshGenerator, sector);
            Mesh wallMesh = CreateWall(meshGenerator, sector);
            Mesh floorMesh = CreateFloor(meshGenerator, sector);
            return new MapMeshes(ceilingMesh: ceilingMesh, wallMesh: wallMesh, floorMesh: floorMesh);
        }

        Mesh CreateCeiling(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh ceilingMesh = meshGenerator.GetCeilingMesh();
            CreateObjectFromMesh(ceilingMesh, "Ceiling", sector, ceilingMaterial);
            return ceilingMesh;
        }

        Mesh CreateWall(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh wallMesh = meshGenerator.GetWallMesh();
            GameObject wall = CreateObjectFromMesh(wallMesh, "Walls", sector, wallMaterial);
            AddMeshCollider(wall, wallMesh);
            return wallMesh;
        }

        Mesh CreateFloor(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh floorMesh = meshGenerator.GetFloorMesh();
            GameObject floor = CreateObjectFromMesh(floorMesh, "Floor", sector, floorMaterial);
            AddMeshCollider(floor, floorMesh);
            return floorMesh;
        }

        void AddMeshCollider(GameObject gameObject, Mesh mesh)
        {
            MeshCollider collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }
    } 
}