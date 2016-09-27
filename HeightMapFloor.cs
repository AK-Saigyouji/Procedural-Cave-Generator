namespace CaveGeneration.HeightMaps
{
    public sealed class HeightMapFloor : HeightMapBuilder
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
