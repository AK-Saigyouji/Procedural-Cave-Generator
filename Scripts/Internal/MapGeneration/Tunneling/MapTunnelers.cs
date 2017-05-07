using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Contains various strategies for creating tunnels between two points in a map. 
    /// </summary>
    public static class MapTunnelers
    {
        public static void CarveDirectTunnel(Map map, Coord start, Coord end, int tunnelingRadius)
        {
            foreach (Coord tile in start.GetLineTo(end))
            {
                ClearNeighbours(map, tile, tunnelingRadius);
            }
        }

        /// <summary>
        /// Replace nearby tiles with floors.
        /// </summary>
        static void ClearNeighbours(Map map, Coord center, int radius)
        {
            // Ensure we don't step off the map and into an index exception
            int xMin = Mathf.Max(0, center.x - radius);
            int yMin = Mathf.Max(0, center.y - radius);
            int xMax = Mathf.Min(map.Length - 1, center.x + radius);
            int yMax = Mathf.Min(map.Width - 1, center.y + radius);
            // Look at each x,y in a square surrounding the center, but only remove those that fall within
            // the circle of given radius. 
            int squaredRadius = radius * radius;
            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    if (IsInCircle(new Coord(x, y), center, squaredRadius))
                    {
                        map[x, y] = Tile.Floor;
                    }
                }
            }
        }

        static bool IsInCircle(Coord testCoord, Coord center, int squaredRadius)
        {
            return center.SquaredDistance(testCoord) <= squaredRadius;
        }
    } 
}