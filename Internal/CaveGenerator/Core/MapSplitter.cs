using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CaveGeneration.MapGeneration;

namespace CaveGeneration
{
    /// <summary>
    /// Divides Map objects into chunks of size given by the chunksize. i.e. if CHUNK_SIZE = 100, then it will
    /// break the maps into chunks of up to 100 by 100.
    /// </summary>
    static class MapSplitter
    {
        public const int CHUNK_SIZE = 150;

        /// <summary>
        /// Divide the map into smaller Map chunks.
        /// </summary>
        /// <returns>Returns a readonly list of smaller Map objects.</returns>
        public static IList<Map> Subdivide(Map map)
        {
            List<Map> maps = new List<Map>();
            int xNumComponents = Mathf.CeilToInt(map.Length / (float)CHUNK_SIZE);
            int yNumComponents = Mathf.CeilToInt(map.Width / (float)CHUNK_SIZE);
            for (int x = 0; x < xNumComponents; x++)
            {
                for (int y = 0; y < yNumComponents; y++)
                {
                    Map subMap = GenerateSubMap(map, x, y);
                    maps.Add(subMap);
                }
            }
            return maps.AsReadOnly();
        }

        static Map GenerateSubMap(Map map, int xIndex, int yIndex)
        {
            int xStart = xIndex * CHUNK_SIZE, yStart = yIndex * CHUNK_SIZE;
            int xEnd = ComputeSubmapEndPoint(xStart, map.Length);
            int yEnd = ComputeSubmapEndPoint(yStart, map.Width);

            int scale = map.SquareSize;
            int length = xEnd - xStart;
            int width = yEnd - yStart;
            Vector3 position = new Vector3(xStart, 0f, yStart) * scale;
            Coord index = new Coord(xIndex, yIndex);
            Map subMap = new Map(length, width, scale, index, position);

            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    subMap[x - xStart, y - yStart] = map[x, y];
                }
            }
            return subMap;
        }

        static int ComputeSubmapEndPoint(int start, int max)
        {
            return (start + CHUNK_SIZE >= max) ? max : start + CHUNK_SIZE + 1;
        }
    }

}