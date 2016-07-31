using UnityEngine;

namespace CaveGeneration
{
    public class Noise
    {
        Vector2[] offsets;
        float[] amplitudes;
        float[] frequencies;

        float scale;

        const int OFFSET_MINIMUM = -100000;
        const int OFFSET_MAXIMUM = 100000;

        public Noise(int numLayers, float amplitudePersistance, float frequencyGrowth, float scale, int seed)
        {
            this.scale = scale;
            offsets = CreateOffsets(seed, numLayers);
            amplitudes = GetArrayOfExponents(amplitudePersistance, numLayers);
            frequencies = GetArrayOfExponents(frequencyGrowth, numLayers);
        }

        public float GetHeight(float x, float y)
        {
            float height = 0f;
            for (int i = 0; i < amplitudes.Length; i++)
            {
                Vector2 sample = offsets[i] + (new Vector2(x, y)) * frequencies[i] / scale;
                float perlinValue = Mathf.PerlinNoise(sample.x, sample.y) * 2 - 1; // transform range from (0,1) to (-1,1)
                height += perlinValue * amplitudes[i];
            }
            return Mathf.InverseLerp(-1f, 1f, height);
        }

        Vector2[] CreateOffsets(int seed, int numLayers)
        {
            Vector2[] offsets = new Vector2[numLayers];
            System.Random random = new System.Random(seed);
            for (int i = 0; i < numLayers; i++)
            {
                float x = random.Next(OFFSET_MINIMUM, OFFSET_MAXIMUM);
                float y = random.Next(OFFSET_MINIMUM, OFFSET_MAXIMUM);
                offsets[i] = new Vector2(x, y);
            }
            return offsets;
        }

        float[] GetArrayOfExponents(float factor, int length)
        {
            float[] exponents = new float[length];
            for (int i = 0; i < length; i++)
            {
                exponents[i] = Mathf.Pow(factor, i);
            }
            return exponents;
        }
    }
}