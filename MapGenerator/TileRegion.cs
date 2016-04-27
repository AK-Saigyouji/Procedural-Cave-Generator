using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MapHelpers
{
    class TileRegion : IEnumerable<Coord>
    {
        List<Coord> tiles = new List<Coord>();
        public int Size { get { return tiles.Count; } }

        public void Add(Coord coord)
        {
            tiles.Add(coord);
        }

        public Coord this[int index]
        {
            get { return tiles[index]; }
        }

        public IEnumerator<Coord> GetEnumerator()
        {
            for (int i = 0; i < tiles.Count; ++i)
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
