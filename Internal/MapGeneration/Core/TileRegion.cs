/* The main motivation for creating this class was to avoid having objects of type "List<List<Coord>>" or "List<Coord[]>" 
 * in the code. Thus the class exposes exposes functionality that is a struct subset of what would be provided by 
 * by an array of Coords. */

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// A simple, readonly collection of tiles in the map.
    /// </summary>
    sealed class TileRegion : IEnumerable<Coord>
    {
        Coord[] tiles;
        public int Count;

        public TileRegion(List<Coord> tiles)
        {
            Coord[] tilesCopy = new Coord[tiles.Count];
            for (int i = 0; i < tiles.Count; i++)
            {
                tilesCopy[i] = tiles[i];
            }
            this.tiles = tilesCopy;
            Count = tiles.Count;
        }

        public Coord this[int index]
        {
            get { return tiles[index]; }
        }

        public IEnumerator<Coord> GetEnumerator()
        {
            for (int i = 0; i < tiles.Length; ++i)
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
