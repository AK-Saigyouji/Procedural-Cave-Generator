using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MapGenerator : MonoBehaviour {
    [SerializeField]
    int width;
    [SerializeField]
    int height;
    [SerializeField]
    [Range(0,1)]
    float randomFillPercent;
    [SerializeField]
    string seed;
    [SerializeField]
    bool useRandomSeed;

    int[,] map;

    int SMOOTHING_ITERATIONS = 5;
    int CELLULAR_THRESHOLD = 4;
    int BORDER_SIZE = 5;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();
        SmoothMap(SMOOTHING_ITERATIONS);
        int[,] borderedMap = GetBorderedMap(map, BORDER_SIZE);
        GetComponent<MeshGenerator>().generateMesh(borderedMap);

    }

    int[,] GetBorderedMap(int[,] map, int borderSize)
    {
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];
        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                int xShifted = x - borderSize;
                int yShifted = y - borderSize;
                bool isInsideBorder = (0 <= xShifted && xShifted < width) && (0 <= yShifted && yShifted < height);
                borderedMap[x, y] = isInsideBorder ? map[xShifted, yShifted] : 1;
            }
        }
        return borderedMap;
    }

    void RandomFillMap()
    {
        Random.seed = (useRandomSeed ? Time.time.ToString() : seed).GetHashCode();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                bool isEdge = (x == 0 || x == width - 1 || y == 0 || y == height - 1);
                map[x,y] = (isEdge || Random.value < randomFillPercent) ? 1 : 0;
            }
    }

    void SmoothMap(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            int[,] newMap = (int[,])map.Clone();

            for (int x = 1; x < width-1; x++)
                for (int y = 1; y < height-1; y++)
                {
                    int neighborCount = GetSurroundingWallCount(x, y);
                    if (neighborCount != CELLULAR_THRESHOLD)
                        newMap[x, y] = (neighborCount > CELLULAR_THRESHOLD) ? 1 : 0;
                }

            map = newMap;
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int x = gridX - 1; x <= gridX + 1; x++)
            for (int y = gridY - 1; y <= gridY + 1; y++)
                if (x != gridX || y != gridY)
                    wallCount += map[x, y];
        return wallCount;
    }
}
