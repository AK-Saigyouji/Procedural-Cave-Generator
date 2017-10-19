/* This module was created to allow for the customization of cave wall geometry. When using large cave walls, the flat
 * walls would become very noticeably aesthetically displeasing. By increasing the number of vertices on the walls (spread 
 * roughly uniformly between the original vertices) and displacing the vertices (e.g. pushing them in or out)
 * we can get far more interesting and natural looking cave walls. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.Modules.CaveWalls
{
    /// <summary>
    /// Base type for modules responsible for modifying the geometry of cave walls, by specifying the number of vertices
    /// along the walls and modifying their positions.
    /// </summary>
    public abstract class CaveWallModule : Module
    {
        protected const string fileName = "Wall Module";
        protected const string rootMenupath = MODULE_ASSET_PATH + "Wall Modules/";

        /// <summary>
        /// Returns the simplest cave wall module, which produces flat walls with the minimum amount of geometry. 
        /// </summary>
        public static CaveWallModule Default { get { return CreateInstance<CaveWallFlat>(); } }

        /// <summary>
        /// How many vertices will be added to each corner in the wall.
        /// Must be non-negative. Avoid using any more than necessary, as this will result in more vertices and triangles
        /// in the resulting wall mesh. Setting to too large of a number exceed Unity's vertex limit for meshes, resulting
        /// in an exception.
        /// </summary>
        public abstract int ExtraVerticesPerCorner { get; }

        /// <summary>
        /// Adjust the position of this vertex in the original flat wall. Normally only the x and z values of 
        /// original should be altered: altering y runs the risk of degenerate triangles in the wall mesh, so modify
        /// with caution. Will only be called on the added vertices: the original floor and ceiling vertices cannot
        /// be modified. 
        /// </summary>
        /// <param name="original">The original wall vertex.</param>
        /// <param name="normal">The direction orthogonal to the wall's corner, pointing outwards away from the wall.</param>
        /// <param name="floorHeight">The height of the floor at this point.</param>
        /// <param name="ceilingHeight">The height of the ceiling at this point.</param>
        /// <returns>The new vertex replacing original in the mesh.</returns>
        public abstract Vector3 GetAdjustedCorner(VertexContext context);
    }
}