using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Carves out tunnels between pairs of coordinates on a map.
    /// </summary>
    public interface ITunneler
    {
        /// <summary>
        /// Carve out a tunnel between the pair of coordinates (inclusive) in the given map.
        /// </summary>
        void CarveTunnel(Map map, Coord start, Coord end);
    }

    /// <summary>
    /// Provides various strategies for creating tunnels between two points in a map. 
    /// </summary>
    public static class MapTunnelers
    {
        public static ITunneler GetRandomDirectedTunneler(Coord boundary, int tunnelingRadius, int seed)
        {
            return new RandomDirectedWalker(boundary, seed, tunnelingRadius);
        }

        public static ITunneler GetDirectTunneler(Map map, int tunnelingRadius)
        {
            return new DirectTunneler(tunnelingRadius);
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

        private sealed class DirectTunneler : ITunneler
        {
            readonly int radius;

            public DirectTunneler(int radius)
            {
                this.radius = radius;
            }

            public void CarveTunnel(Map map, Coord start, Coord end)
            {
                foreach (Coord tile in start.GetLineTo(end))
                {
                    ClearNeighbours(map, tile, radius);
                }
            }
        }

        /// <summary>
        /// Produces random walks between pairs of points, such that points gravitate towards the goal. 
        /// </summary>
        private sealed class RandomDirectedWalker : ITunneler
        {
            readonly Coord[] directions = new [] 
            {
                new Coord(1, 0), new Coord(0, 1), new Coord(-1, 0), new Coord(0, -1),
                new Coord(1, 1), new Coord(-1, 1), new Coord(1, -1), new Coord(-1, -1)
            };
            readonly System.Random random;
            readonly int xBoundary;
            readonly int yBoundary;
            readonly int radius;

            /// <param name="boundary">Corresponds to the coordinate which is just outside (top right) of the 
            /// maximum possible coordinates. Equal to (length, width) of the map.</param>
            /// <param name="seed">Fixes the randomness.</param>
            /// <param name="radius">Determines the width of carved tunnels.</param>
            public RandomDirectedWalker(Coord boundary, int seed, int radius)
            {
                this.radius = radius;
                xBoundary = boundary.x;
                yBoundary = boundary.y;
                random = new System.Random(seed);
            }

            public void CarveTunnel(Map map, Coord start, Coord end)
            {
                Coord current = start;
                ClearNeighbours(map, current, radius);
                while (current != end)
                {
                    current = GetNextDirection(current, end);
                    ClearNeighbours(map, current, radius);
                }
            }

            Coord GetNextDirection(Coord current, Coord end)
            {
                foreach (Coord direction in GetRandomDirections())
                {
                    Coord next = current + direction;
                    if (current.SupNormDistance(end) >= next.SupNormDistance(end) && IsInBounds(next))
                    {
                        return next;
                    }
                }
                throw new InvalidOperationException();
            }

            bool IsInBounds(Coord coord)
            {
                int x = coord.x;
                int y = coord.y;
                return 0 <= x && x < xBoundary && 0 <= y && y < yBoundary;
            }

            Coord[] GetRandomDirections()
            {
                for (int i = 0; i < directions.Length; i++)
                {
                    Swap(i, random.Next(i, directions.Length));
                }
                return directions;
            }

            void Swap(int i, int j)
            {
                Coord temp = directions[i];
                directions[i] = directions[j];
                directions[j] = temp;
            }
        }
    } 
}