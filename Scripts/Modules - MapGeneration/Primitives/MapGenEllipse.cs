using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.MapGeneration;
using AKSaigyouji.Maps;

namespace AKSaigyouji.Modules.MapGeneration
{
    [CreateAssetMenu(fileName = "Ellipse", menuName = rootMenupath + "Ellipse")]
    public sealed class MapGenEllipse : MapGenModule
    {
        /// <summary>
        /// Horizontal length of n corresponds to an ellipse with total length 2n + 1. 
        /// </summary>
        public int Horizontal
        {
            get { return horizontal; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");
                horizontal = value;
            }
        }

        /// <summary>
        /// Vertical length of n corresponds to an ellipse with total height 2n + 1. 
        /// </summary>
        public int Vertical
        {
            get { return vertical; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");
                vertical = value;
            }
        }

        [SerializeField] int horizontal;
        [SerializeField] int vertical;

        const int BORDER_SIZE = 1;

        public override Coord GetMapSize()
        {
            int length = 1 + 2 * (horizontal + BORDER_SIZE);
            int width = 1 + 2 * (vertical + BORDER_SIZE);
            return new Coord(length, width);
        }

        public override Map Generate()
        {
            int length = GetTotalLength();
            int width = GetTotalWidth();
            float verticalReciprocal = 1f / vertical;
            float horizontalReciprocal = 1f / horizontal;
            Map map = new Map(length, width);
            map.Fill(Tile.Wall);
            for (int y = 0; y < width; y++)
            {
                float yShifted = (y - vertical) * verticalReciprocal;
                float yy = yShifted * yShifted;
                for (int x = 0; x < length; x++)
                {
                    float xShifted = (x - horizontal) * horizontalReciprocal;
                    float xx = xShifted * xShifted;
                    if (xx + yy < 1)
                    {
                        map[x, y] = Tile.Floor;
                    }
                }
            }
            map = MapBuilder.ApplyBorder(map, BORDER_SIZE);
            return map;
        }

        public override IEnumerable<Coord> GetBoundary()
        {
            // This is definitely not the cleanest way to trace out the boundary of an ellipse, but it's robust in that
            // it will continue working even if we change the generation code.
            var boundary = new HashSet<Coord>();
            float verticalReciprocal = 1f / vertical;
            float horizontalReciprocal = 1f / horizontal;
            int length = GetTotalLength() + 2 * BORDER_SIZE;
            int width = GetTotalWidth() + 2 * BORDER_SIZE;
            bool foundFloors;
            for (int y = 0; y < width; y++)
            {
                foundFloors = false;
                float yShifted = (y - vertical) * verticalReciprocal;
                float yy = yShifted * yShifted;
                for (int x = 0; x < length; x++)
                {
                    float xShifted = (x - horizontal) * horizontalReciprocal;
                    float xx = xShifted * xShifted;
                    if (xx + yy < 1 && !foundFloors)
                    {
                        foundFloors = true;
                        boundary.Add(new Coord(x - 1, y)); // first floor tile in this row: return the tile to the left.
                    }
                    else if (xx + yy >= 1 && foundFloors)
                    {
                        foundFloors = false;
                        boundary.Add(new Coord(x, y)); // first wall tile after the row of floor tiles: return this tile
                    }
                }
            }
            for (int x = 0; x < length; x++)
            {
                foundFloors = false;
                float xShifted = (x - horizontal) * horizontalReciprocal;
                float xx = xShifted * xShifted;
                for (int y = 0; y < width; y++)
                {
                    float yShifted = (y - vertical) * verticalReciprocal;
                    float yy = yShifted * yShifted;
                    if (yy + xx < 1 && !foundFloors)
                    {
                        foundFloors = true;
                        boundary.Add(new Coord(x, y - 1));
                    }
                    else if (yy + xx >= 1 && foundFloors)
                    {
                        foundFloors = false;
                        boundary.Add(new Coord(x, y));
                    }
                }
            }
            return boundary;
        }

        int GetTotalLength()
        {
            return 1 + 2 * horizontal;
        }

        int GetTotalWidth()
        {
            return 1 + 2 * vertical;
        }

        void OnValidate()
        {
            horizontal = Math.Max(0, horizontal);
            vertical = Math.Max(0, vertical);
        }
    } 
}