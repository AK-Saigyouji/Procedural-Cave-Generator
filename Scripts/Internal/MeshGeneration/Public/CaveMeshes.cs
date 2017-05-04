/* This class stores the output of the mesh generator system, and is designed to hide a bit of complexity 
 having to do with the fact that Meshes cannot be created or manipulated outside of the main thread. Originally,
 using the cave generator required three steps: creating a mesh generator, calling a generate method which did the
 work of determining the cave geometry, and then extracting the meshes which built actual Mesh objects. 
 The first two steps could be done on multiple threads, but the third had to be done on the main thread.
 
  Ideally, the mesh generator should be a static, stateless class, and generation should be doable with a single
 static method. This class was designed to facilitate this, through lazy initialization. Instead of returning
 the meshes, the cave generator returns this object, which builds the meshes when they're requested.*/

using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Output of the mesh generator, containing the meshes needed to produce a cave.
    /// </summary>
    public sealed class CaveMeshes
    {
        Mesh floorMesh;
        Mesh wallMesh;
        Mesh ceilingMesh;

        MeshData floorData;
        MeshData wallData;
        MeshData ceilingData;

        internal CaveMeshes(MeshData floor, MeshData walls, MeshData ceiling)
        {
            floorData = floor;
            wallData = walls;
            ceilingData = ceiling;
        }

        public Mesh ExtractFloorMesh()
        {
            if (floorMesh == null)
            {
                floorMesh = floorData.CreateMesh();
                floorData = null;
            }
            return floorMesh;
        }

        public Mesh ExtractWallMesh()
        {
            if (wallMesh == null)
            {
                wallMesh = wallData.CreateMesh();
                wallData = null;
            }
            return wallMesh;
        }

        public Mesh ExtractCeilingMesh()
        {
            if (ceilingMesh == null)
            {
                ceilingMesh = ceilingData.CreateMesh();
                ceilingData = null;
            }
            return ceilingMesh;
        }
    } 
}