using AKSaigyouji.ArrayExtensions;
using AKSaigyouji.Maps;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AKSaigyouji.GraphAlgorithms
{
    /// <summary>
    /// Class dedicated to enlarging floors within a map so that larger objects can navigate the map.
    /// </summary>
    public sealed class PassageEnlarger
    {
        // These are cached as ExpandFloors may need to be called multiple times, and these arrays may be large
        // enough to sit on the large object heap. 
        readonly bool[,] cachedBoolArrayOne;
        readonly bool[,] cachedBoolArrayTwo;
        readonly int[,] cachedIntArray;

        readonly int length;
        readonly int width;

        readonly Coord[] cachedNeighbours = new Coord[4];

        public PassageEnlarger(int length, int width)
        {
            this.length = length;
            this.width = width;
            cachedBoolArrayOne = new bool[length, width];
            cachedBoolArrayTwo = new bool[length, width];
            cachedIntArray = new int[length, width];
        }

        public void ExpandFloors(Map map, int radius)
        {
            if (radius < 0)
                throw new ArgumentOutOfRangeException("radius");

            if (map.Length != length || map.Width != width)
                throw new ArgumentException("Map dimensions must match dimensions passed to constructor.");

            // a tile is good if it can fit an object with the given radius, if it's close enough to such a tile,
            // or if it's a wall.
            bool[,] goodTiles = cachedBoolArrayOne;
            map.ToBoolArray(Tile.Wall, goodTiles);
            goodTiles.ForEach((x, y) =>
            {
                if (CanFitBox(map, x, y, radius))
                {
                    FillBox(goodTiles, x, y, radius, true);
                }
            });
            bool[,] badTiles = Invert(goodTiles);
            int[,] labelledRegions = LabelRegions(badTiles);
            foreach (List<Coord> region in GetRegionsToExpand(goodTiles, labelledRegions))
            {
                region.ForEach(coord => ClearMap(map, coord, radius));
            }
        }

        IEnumerable<List<Coord>> GetRegionsToExpand(bool[,] goodTiles, int[,] labelledRegions)
        {
            const int minRegionSize = 3;
            var regionsToExpand = new List<List<Coord>>();
            goodTiles.ForEach((x, y) =>
            {
                if (!goodTiles[x, y])
                {
                    List<Coord> region = BFS.GetConnectedRegion(x, y, goodTiles);
                    if (region.Count > minRegionSize || NeighboursMultipleRegions(region, labelledRegions))
                    {
                        regionsToExpand.Add(region);
                    }
                }
            });
            return regionsToExpand;
        }

        bool NeighboursMultipleRegions(List<Coord> region, int[,] regions)
        {
            Boundary boundary = new Boundary(regions.GetLength(0), regions.GetLength(1));
            Coord[] neighbours = cachedNeighbours;
            int neighbourType = 0; // this tracks the first neighbour: 0 represents no neighbour
            for (int i = 0; i < region.Count; i++)
            {
                Coord coord = region[i];
                neighbours[0] = coord.Left;
                neighbours[1] = coord.Right;
                neighbours[2] = coord.Up;
                neighbours[3] = coord.Down;
                foreach (Coord neighbour in neighbours)
                {
                    if (boundary.IsInBounds(neighbour))
                    {
                        int type = regions[neighbour.x, neighbour.y];
                        if (type != 0 && type != neighbourType)
                        {
                            if (neighbourType == 0)
                            {
                                neighbourType = type;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        bool[,] Invert(bool[,] booleans)
        {
            bool[,] inverted = cachedBoolArrayTwo;
            for (int y = 0; y < booleans.GetLength(1); y++)
            {
                for (int x = 0; x < booleans.GetLength(0); x++)
                {
                    inverted[x, y] = !booleans[x, y];
                }
            }
            return inverted;
        }

        int[,] LabelRegions(bool[,] tilesToExpand)
        {
            // int was chosen in case the room is large and chaotic enough, having more than 256 or 65535 rooms.
            // a possible future optimization is to offer byte and ushort versions for smaller maps.
            int[,] regions = cachedIntArray;
            Array.Clear(regions, 0, regions.Length);
            int currentLevel = 1;
            tilesToExpand.ForEach((x, y) =>
            {
                if (!tilesToExpand[x, y])
                {
                    List<Coord> region = BFS.GetConnectedRegion(x, y, tilesToExpand);
                    FillRegion(regions, region, currentLevel);
                    currentLevel++;
                }
            });
            return regions;
        }

        void FillRegion(int[,] tiles, List<Coord> region, int value)
        {
            for (int i = 0; i < region.Count; i++)
            {
                Coord coord = region[i];
                tiles[coord.x, coord.y] = value;
            }
        }

        void ClearMap(Map map, Coord center, int radius)
        {
            int rr = radius * radius;
            Boundary bdry = new Boundary(1, map.Length - 2, 1, map.Width - 2);
            for (int y = -radius; y <= radius; y++)
            {
                int yy = y * y;
                int yMap = center.y + y;
                for (int x = -radius; x <= radius; x++)
                {
                    int xx = x * x;
                    int xMap = center.x + x;
                    if (bdry.IsInBounds(xMap, yMap) && xx + yy <= rr)
                    {
                        map[xMap, yMap] = Tile.Floor;
                    }
                }
            }
        }

        void FillBox(bool[,] tiles, int xCenter, int yCenter, int radius, bool value)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int yMap = yCenter + y;
                for (int x = -radius; x <= radius; x++)
                {
                    int xMap = xCenter + x;
                    tiles[xMap, yMap] = value;
                }
            }
        }

        bool CanFitBox(Map map, int xCenter, int yCenter, int radius)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int yMap = yCenter + y;
                for (int x = -radius; x <= radius; x++)
                {
                    int xMap = xCenter + x;
                    if (map.IsWallOrVoid(xMap, yMap))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    } 
}