using AKSaigyouji.Maps;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AKSaigyouji.MapGeneration
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
        /// <summary>
        /// Represents how far the tunneler will deviate from the straight path. 
        /// </summary>
        public enum Variance { Low, High };

        /// <param name="boundary">Represents a boundary for the path: it will not step outside the boundary.</param>
        public static ITunneler GetRandomDirectedTunneler(Boundary boundary, int seed, Variance variance)
        {
            switch (variance)
            {
                case Variance.Low:
                    return new RandomDirectedWalker(boundary, seed);
                case Variance.High:
                    return new RandomWalk(boundary, seed);
                default:
                    throw new ArgumentException("Unrecognized variability.");
            }
        }

        public static ITunneler GetDirectTunneler(Map map)
        {
            return new DirectTunneler();
        }

        sealed class DirectTunneler : ITunneler
        {
            public IEnumerable<Coord> GetPath(Coord start, Coord end)
            {
                return start.GetLineTo(end);
            }
        }

        /// <summary>
        /// Produces random walks between pairs of points, such that points gravitate towards the goal. 
        /// </summary>
        sealed class RandomDirectedWalker : ITunneler
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
                random = new Random(seed);
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
                int timeout = 0;
                Coord next;
                do
                {
                    next = current + directions[random.Next(0, directions.Length)];
                    if (timeout++ > 30000)
                    {
                        // This should be impossible, but is placed here just in case, to avoid an infinite loop.
                        throw new InvalidOperationException("Infinite loop aborted: cannot find a path.");
                    }
                } while (!boundary.IsInBounds(next) || Distance(next, end) > Distance(current, end));

                return next;
            }

            float Distance(Coord a, Coord b)
            {
                return a.SquaredDistance(b);
            }
        }

        sealed class RandomWalk : ITunneler
        {
            readonly Random random;
            readonly Boundary boundary;
            readonly RandomDirectedWalker tunneler;

            const int JUMP_SIZE = 20;

            public RandomWalk(Boundary boundary, int seed)
            {
                random = new Random(seed);
                this.boundary = boundary;
                tunneler = new RandomDirectedWalker(boundary, seed);
            }

            public IEnumerable<Coord> GetPath(Coord start, Coord end)
            {
                Coord current = start;
                var path = Enumerable.Empty<Coord>();
                while (current.Distance(end) > JUMP_SIZE)
                {
                    Coord next;
                    do
                    {
                        next = GetRandomPointOnBox(current, JUMP_SIZE);
                    } while (!boundary.IsInBounds(next) || next.Distance(end) >= current.Distance(end));
                    path = path.Concat(tunneler.GetPath(current, next));
                    current = next;
                }
                path = path.Concat(tunneler.GetPath(current, end));
                return path;
            }

            Coord GetRandomPointOnBox(Coord center, int width)
            {
                return new Coord(
                    random.Next(center.x - width, center.x + width),
                    random.Next(center.y - width, center.y + width)
                    );
            }
        }
    } 
}