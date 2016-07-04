using System;
using System.Linq;
using System.Collections.Generic;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// A connected, open space in the map, keeping track of which of its tiles are adjacent to walls.
    /// </summary>
    class Room : IComparable<Room>
    {
        public TileRegion allTiles { get; private set; }
        /// <summary>
        /// All floor tiles in this room that have an adjacent wall tile. 
        /// </summary>
        public TileRegion edgeTiles { get; private set; }
        public int Count { get; private set; }

        public Room(TileRegion region, Map map)
        {
            allTiles = region;
            Count = region.Count;
            var edgeTiles = GetEdgeTiles(map);
            this.edgeTiles = SortEdgeTiles(edgeTiles);
        }

        IEnumerable<Coord> GetEdgeTiles(Map map)
        {
            return allTiles.Where(tile => map.IsEdgeTile(tile));
        }

        TileRegion SortEdgeTiles(IEnumerable<Coord> edgeTiles)
        {
            var visited = edgeTiles.ToDictionary(tile => tile, tile => false);
            TileRegion sortedEdges = new TileRegion();
            Stack<Coord> stack = new Stack<Coord>();

            Coord firstTile = edgeTiles.First();
            stack.Push(firstTile);
            sortedEdges.Add(firstTile);
            visited[firstTile] = true;

            while (stack.Count > 0)
            {
                Coord tile = stack.Pop();
                foreach (Coord adjacentTile in GetAdjacentCoords(tile))
                {
                    if (visited.ContainsKey(adjacentTile) && !visited[adjacentTile])
                    {
                        visited[adjacentTile] = true;
                        stack.Push(adjacentTile);
                        sortedEdges.Add(adjacentTile);
                    }
                }
            }
            return sortedEdges;
        }

        IEnumerable<Coord> GetAdjacentCoords(Coord tile)
        {
            Coord left = tile.left;
            Coord right = tile.right;

            yield return tile.up;
            yield return tile.down;
            yield return right;
            yield return left;
            yield return left.up;
            yield return left.down;
            yield return right.up;
            yield return right.down;
        }

        public int CompareTo(Room otherRoom)
        {
            return Count.CompareTo(otherRoom.Count);
        }

        public override string ToString()
        {
            return string.Format("Room with {0} tiles, {1} on the edge.", allTiles.Count, edgeTiles.Count);
        }
    }
}