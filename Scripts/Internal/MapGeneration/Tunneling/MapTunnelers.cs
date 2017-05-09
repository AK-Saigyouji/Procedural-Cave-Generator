using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Builds a tunnel/path between two coordinates.
    /// </summary>
    public interface ITunneler
    {
        /// <summary>
        /// Get a path between start (inclusive) and end (inclusive). 
        /// </summary>
        IEnumerable<Coord> GetPath(Coord start, Coord end);
    }

    /// <summary>
    /// Provides various strategies for creating tunnels between two points in a map. 
    /// </summary>
    public static class MapTunnelers
    {
        /// <param name="boundary">Represents out of bounds coordinates. </param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ITunneler GetRandomDirectedTunneler(Boundary boundary, int seed)
        {
            return new RandomDirectedWalker(boundary, seed);
        }

        public static ITunneler GetDirectTunneler(Map map)
        {
            return new DirectTunneler();
        }

        private sealed class DirectTunneler : ITunneler
        {
            public IEnumerable<Coord> GetPath(Coord start, Coord end)
            {
                return start.GetLineTo(end);
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
            readonly Boundary boundary;

            /// <param name="boundary">Corresponds to the coordinate which is just outside (top right) of the 
            /// maximum possible coordinates. Equal to (length, width) of the map.</param>
            /// <param name="seed">Fixes the randomness.</param>
            public RandomDirectedWalker(Boundary boundary, int seed)
            {
                this.boundary = boundary;
                random = new System.Random(seed);
            }

            public IEnumerable<Coord> GetPath(Coord start, Coord end)
            {
                Coord current = start;
                yield return current;
                while (current != end)
                {
                    current = GetNextDirection(current, end);
                    yield return current;
                }
            }

            Coord GetNextDirection(Coord current, Coord end)
            {
                foreach (Coord direction in GetRandomDirections())
                {
                    Coord next = current + direction;
                    if (current.SupNormDistance(end) >= next.SupNormDistance(end) && boundary.IsInBounds(next))
                    {
                        return next;
                    }
                }
                throw new InvalidOperationException();
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