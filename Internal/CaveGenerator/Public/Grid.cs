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

        internal Grid(Map map) : this(map.Length, map.Width)
        {
            System.Array.Copy(map.ToByteArray(), tiles, tiles.Length);
        }

        public Grid(byte[,] tiles) : this(tiles.GetLength(0), tiles.GetLength(1))
        {
            if (!AreValidTiles(tiles))
            {
                throw new System.ArgumentException("Tiles must be 0 (floors) and 1 (walls).", "tiles");
            }
            System.Array.Copy(tiles, this.tiles, tiles.Length);
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
    } 
}