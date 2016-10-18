/* The finalized grid as exposed to users of the CaveGeneration system. Note that the Tile enum defined here is different
 from the Tile enum defined in MapGeneration, as the latter is internal to the MapGeneration system while this one is intended
 for external consumption.*/

using Map = CaveGeneration.MapGeneration.Map;

namespace CaveGeneration
{
    /// <summary>
    /// Simple enum representing a floor or wall.
    /// </summary>
    public enum Tile : byte
    {
        Floor = 0,
        Wall = 1
    }

    /// <summary>
    /// A readonly, two dimensionsal grid of tiles.
    /// </summary>
    public sealed class Grid
    {
        readonly Tile[,] tiles;

        /// <summary>
        /// Number of tiles in the x direction.
        /// </summary>
        public int Length { get { return length; } }

        /// <summary>
        /// Number of tiles in the z direction.
        /// </summary>
        public int Width { get { return width; } }

        readonly int length;
        readonly int width;

        Grid(int length, int width)
        {
            this.length = length;
            this.width = width;
            tiles = new Tile[length, width];
        }

        internal Grid(Map map)
        {
            if (map == null) throw new System.ArgumentNullException("map");

            length = map.Length;
            width = map.Width;
            tiles = new Tile[length, width];
            Copy(map.ToByteArray());
        }

        public Grid(byte[,] tiles)
        {
            if (tiles == null) throw new System.ArgumentNullException("tiles");
            if (!AreValidTiles(tiles))
                throw new System.ArgumentException("Tiles must be 0 (floors) and 1 (walls).", "tiles");

            length = tiles.GetLength(0);
            width = tiles.GetLength(1);
            this.tiles = new Tile[length, width];
            Copy(tiles);
        }

        public Tile this[int x, int y]
        {
            get { return tiles[x, y]; }
        }

        // Ensures every tile is either 0 or 1, by ORing them all together. 0 and 1 have 0s for every bit except the final one.
        // Thus their total OR will have this property unless there is a number >= 2 somewhere in the array.
        bool AreValidTiles(byte[,] tiles)
        {
            int length = tiles.GetLength(0);
            int width = tiles.GetLength(1);
            int accumulator = 0;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    accumulator |= tiles[x, y];
                }
            }
            return accumulator < 2;
        }

        void Copy(byte[,] tiles)
        {
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    this.tiles[x, y] = (Tile)tiles[x, y];
                }
            }
        }
    } 
}