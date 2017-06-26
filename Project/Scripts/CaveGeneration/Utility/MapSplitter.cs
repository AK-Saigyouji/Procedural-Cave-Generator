using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AKSaigyouji.Maps;

namespace AKSaigyouji.CaveGeneration
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
        public static Map[,] Subdivide(Map map)
        {
            int xNumComponents = Mathf.CeilToInt(map.Length / (float)CHUNK_SIZE);
            int yNumComponents = Mathf.CeilToInt(map.Width / (float)CHUNK_SIZE);
            Map[,] mapChunks = new Map[xNumComponents, yNumComponents];
            for (int x = 0; x < xNumComponents; x++)
            {
                for (int y = 0; y < yNumComponents; y++)
                {
                    mapChunks[x, y] = GenerateSubMap(map, x, y);
                }
            }
            return mapChunks;
        }

        static Map GenerateSubMap(Map map, int xIndex, int yIndex)
        {
            int xStart = xIndex * CHUNK_SIZE;
            int yStart = yIndex * CHUNK_SIZE;
            int xEnd = ComputeSubmapEndPoint(xStart, map.Length);
            int yEnd = ComputeSubmapEndPoint(yStart, map.Width);

            int length = xEnd - xStart;
            int width = yEnd - yStart;
            
            Map subMap = new Map(length, width);

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
            if (start + CHUNK_SIZE >= max)
            {
                return max;
            }
            else
            {
                return start + CHUNK_SIZE + 1;
            }
        }
    }
}