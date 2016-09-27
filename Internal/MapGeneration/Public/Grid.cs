/* Readonly version of the Map class intended for external consumption once the structure of Map is finalized.*/

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// A readonly, two dimensionsal grid of tiles.
    /// </summary>
    public sealed class Grid
    {
        readonly Tile[,] tiles;

        /// <summary>
        /// Number of tiles in the x direction.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Number of tiles in the z direction.
        /// </summary>
        public int Width { get; private set; }

        public Grid(int length, int width)
        {
            Length = length;
            Width = width;
            tiles = new Tile[length, width];
        }

        public Grid(Tile[,] tiles) : this(tiles.GetLength(0), tiles.GetLength(1))
        {
            for (int y = 0; y < Width; y++)
            {
                for (int x = 0; x < Length; x++)
                {
                    this.tiles[x, y] = tiles[x, y];
                }
            }
        }

        public Tile this[int x, int y]
        {
            get { return tiles[x, y]; }
        }
    } 
}