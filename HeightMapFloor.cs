﻿namespace CaveGeneration
{
    public class HeightMapFloor : HeightMap
    {
        void Reset()
        {
            maxHeight = 0.5f;
            scale = 50f;
            numLayers = 2;
            amplitudeDecay = 0.5f;
            frequencyGrowth = 2f;
        }
    } 
}
