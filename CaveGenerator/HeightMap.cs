using System;
using System.Collections;
using System.Collections.Generic;
using CaveGeneration;

// Experimental: not currently in use

/// <summary>
/// Used to specify the height for each point in the map based on a given function. 
/// </summary>
public class HeightMap {

    Dictionary<Coord, float> cachedHeights;
    Func<int, int, float> heightFunction;
    int seamValue = Map.maxSubmapSize;

    public HeightMap(Func<int, int, float> heightFunction)
    {
        this.heightFunction = heightFunction;
        cachedHeights = new Dictionary<Coord, float>();
    }

    public float GetHeight(int x, int y)
    {
        if (IsOnSeam(x, y))
        {
            Coord coord = new Coord(x, y);
            if (!cachedHeights.ContainsKey(coord))
            {
                cachedHeights[coord] = heightFunction(x, y);
            }
            return cachedHeights[coord];
        }
        else
        {
            return heightFunction(x, y);
        }
    }

    bool IsOnSeam(int x, int y)
    {
        return x == seamValue || y == seamValue || x == 0 || y == 0;
    }
}