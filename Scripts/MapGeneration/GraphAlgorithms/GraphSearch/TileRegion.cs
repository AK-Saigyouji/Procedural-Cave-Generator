/* The main motivation for creating this class was to avoid having objects of type "List<List<Coord>>" or "List<Coord[]>" 
 * in the code. Thus the class exposes exposes functionality that is a strict subset of what would be provided by 
 * by an array of Coords. */

using AKSaigyouji.Maps;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Assertions;

namespace AKSaigyouji.GraphAlgorithms
{
    /// <summary>
    /// A simple, readonly collection of tiles in the map.
    /// </summary>
    sealed class TileRegion : IEnumerable<Coord>
    {
        Coord[] tiles;
        public int Count { get { return tiles.Length; } }
        public int Index { get; private set; }

        public TileRegion(List<Coord> tiles, int index)
        {
            Assert.IsNotNull(tiles);
            Coord[] tilesCopy = new Coord[tiles.Count];
            for (int i = 0; i < tiles.Count; i++)
            {
                tilesCopy[i] = tiles[i];
            }
            this.tiles = tilesCopy;
            Index = index;
        }

        public Coord this[int index]
        {
            get { return tiles[index]; }
        }

        public IEnumerator<Coord> GetEnumerator()
        {
            for (int i = 0; i < tiles.Length; i++)
                yield return tiles[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (Coord coord in tiles)
            {
                builder.AppendLine(coord.ToString());
            }
            return builder.ToString();
        }
    } 
}
