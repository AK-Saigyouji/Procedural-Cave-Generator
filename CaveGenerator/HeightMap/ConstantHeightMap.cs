using UnityEngine;
using System.Collections;

public class ConstantHeightMap : IHeightMap {

    public int BaseHeight { get; private set; }
    public bool IsSimple { get; private set; }

    public ConstantHeightMap(int wallHeight)
    {
        BaseHeight = wallHeight;
        IsSimple = true;
    }

	public float GetHeight(float x, float y)
    {
        return 0;
    }
}
