using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// A simple class corresponding to a collection of tiles in the map.
    /// </summary>
    class TileRegion : IEnumerable<Coord>
    {
        Coord[] tiles;
        public int Count { get { return tiles.Length; } }

        public TileRegion(List<Coord> tiles)
        {
            this.tiles = new Coord[tiles.Count];
            for (int i = 0; i < tiles.Count; i++)
            {
                this.tiles[i] = tiles[i];
            }
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
