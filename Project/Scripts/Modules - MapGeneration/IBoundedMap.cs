using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;

namespace AKSaigyouji.Modules.MapGeneration
{
    /// <summary>
    /// Interface for Maps with well-defined boundaries. Specifying this extra structure offers significantly improved
    /// visualization and validation capabilities.
    /// </summary>
    public interface IBoundedMap
    {
        /// <summary>
        /// Enumerates a boundary around the map generator such that the floors of any generated map is guaranteed to 
        /// be within the boundary in question. A floor tile should only lie on the perimeter itself if it is also marked
        /// as an opening.
        /// </summary>
        IEnumerable<Coord> GetBoundary();

        /// <summary>
        /// The static openings defined for this map type, i.e. the ones that are guaranteed to be produced by this
        /// configuration, regardless of seed. If the map produces openings unreliably (which is not recommended) then
        /// do not include them.
        /// </summary>
        IEnumerable<MapEntrance> GetOpenings();
    } 
}