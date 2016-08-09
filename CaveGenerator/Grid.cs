namespace CaveGeneration
{
    /// <summary>
    /// A readonly, two dimensionsal grid of tiles approximately representing the walls and floors in a generated cave. 
    /// </summary>
    public class Grid
    {
        readonly Tile[,] tiles;

        public Grid(int length, int width)
        {
            tiles = new Tile[length, width];
        }

        public Grid(Tile[,] tiles)
        {
            int length = tiles.GetLength(0);
            int width = tiles.GetLength(1);
            this.tiles = new Tile[length, width];
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    this.tiles[x, y] = tiles[x, y];
                }
            }
        }

        public Tile this[int x, int y]
        {
            get { return tiles[x, y]; }
        }

        public Tile this[Coord tile]
        {
            get { return tiles[tile.x, tile.y]; }
        }

        public int GetLength(int dimension)
        {
            return tiles.GetLength(dimension);
        }
    } 
}