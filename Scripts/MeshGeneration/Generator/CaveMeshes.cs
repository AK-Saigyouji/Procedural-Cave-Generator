/* This class was designed to package together the meshes needed to produce a three tiered cave. It uses
 lazy initialization to allow the creation of actual meshes to be deferred as long as possible.
 
  None of the components are mandatory, allowing partial caves to be built. This is useful when generating
 the other components using a different technique.*/

using UnityEngine;
using System;

namespace AKSaigyouji.MeshGeneration
{
    /// <summary>
    /// An object containing one or more meshes for a three tier cave.
    /// </summary>
    public sealed class CaveMeshes
    {
        Mesh floorMesh;
        Mesh wallMesh;
        Mesh ceilingMesh;

        MeshData floorData;
        MeshData wallData;
        MeshData ceilingData;

        internal CaveMeshes(MeshData floor = null, MeshData walls = null, MeshData ceiling = null)
        {
            if (floor == null && walls == null && ceiling == null)
                throw new ArgumentException("Must pass at least one non-null mesh.");

            floorData = floor;
            wallData = walls;
            ceilingData = ceiling;
        }

        internal CaveMeshes(Mesh floor = null, Mesh walls = null, Mesh ceiling = null)
        {
            if (floor == null && walls == null && ceiling == null)
                throw new ArgumentException("Must pass at least one non-null mesh.");

            floorMesh = floor;
            wallMesh = walls;
            ceilingMesh = ceiling;
        }

        public bool HasWallMesh { get { return wallData != null || wallMesh != null; } }
        public bool HasFloorMesh { get { return floorData != null || floorMesh != null; } }
        public bool HasCeilingMesh { get { return ceilingData != null || ceilingMesh != null; } }

        // For the following extraction methods, we check if the mesh is defined. If it is, we return it.
        // If it's not, we check for the data needed to build the mesh. If present, we build the mesh,
        // store it, and cache it for future calls. 
        // If both are missing, an exception is thrown.

        public Mesh ExtractFloorMesh()
        {
            if (floorMesh == null)
            {
                if (floorData == null)
                {
                    throw new InvalidOperationException("Does not contain a floor mesh.");
                }
                else
                {
                    floorMesh = floorData.CreateMesh();
                    floorData = null;
                }
            }
            return floorMesh;
        }

        public Mesh ExtractWallMesh()
        {
            if (wallMesh == null)
            {
                if (wallData == null)
                {
                    throw new InvalidOperationException("Does not contain a wall mesh.");
                }
                else
                {
                    wallMesh = wallData.CreateMesh();
                    wallData = null;
                }
            }
            return wallMesh;
        }

        public Mesh ExtractCeilingMesh()
        {
            if (ceilingMesh == null)
            {
                if (ceilingData == null)
                {
                    throw new InvalidOperationException("Does not contain a wall mesh.");
                }
                else
                {
                    ceilingMesh = ceilingData.CreateMesh();
                    ceilingData = null;
                }
            }
            return ceilingMesh;
        }
    } 
}