namespace CaveGeneration
{
    /// <summary>
    /// Height map for the top section of a 3D cave, e.g. ceiling for the CaveGenerator3D or the enclosure for the 
    /// CaveGeneratorEnclosed.
    /// </summary>
    public class HeightMapMain : HeightMap
    {
        public override float GetHeight(float x, float y)
        {
            return maxHeight * noise.GetHeight(x, y);
        }

        void Reset()
        {
            maxHeight = 1f;
            scale = 30f;
            numLayers = 2;
            amplitudeDecay = 0.5f;
            frequencyGrowth = 2f;
        }
    }
}
