using UnityEngine;
using UnityEngine.Assertions;

namespace CaveGeneration
{
    /// <summary>
    /// Random number generator that produces continuously varying numbers between 0 and 1. Offers greater control
    /// and more variation than Perlin Noise, but is otherwise similar. 
    /// </summary>
    sealed class LayeredNoise
    {
        Vector2[] offsets; // Offsets provide randomness to the noise by offsetting the points of perlin noise we sample.
        float[] amplitudes; // Descending coefficients for the contribution of each layer.
        float[] frequencies; // Ascending coefficients for how closely together points of each layer are sampled.

        float scale; // Starting frequency / level of compression.

        const int OFFSET_MINIMUM = -10000;
        const int OFFSET_MAXIMUM = 10000;

        /// <summary>
        /// Create a new random number generator by specifying how layers of Perlin Noise should be added together.
        /// </summary>
        /// <param name="numLayers">How many layers of Perlin Noise should be used.</param>
        /// <param name="amplitudePersistance">Determines the coefficient for each layer. Layer n will receive a 
        /// coefficient of this value raised to the power of n. Values will be clamped to within 0 and 1.</param>
        /// <param name="frequencyGrowth">How compressed subsequent layers should be, i.e. how rapidly values
        /// change as relative to input parameters. Values below 1 will be clamped to 1.</param>
        /// <param name="scale">Initial frequency. Think rolling hills (low scale) vs jagged mountains (high scale).</param>
        /// <param name="seed">Fixes the randomness.</param>
        public LayeredNoise(int numLayers, float amplitudePersistance, float frequencyGrowth, float scale, int seed)
        {
            Assert.IsTrue(scale > 0, "Scale must be positive");
            this.scale = scale;
            offsets = CreateOffsets(seed, numLayers);
            amplitudes = GetArrayOfExponents(amplitudePersistance, numLayers);
            frequencies = GetArrayOfExponents(frequencyGrowth, numLayers);
        }

        /// <summary>
        /// Create a random number generator with a single layer of Perlin Noise. 
        /// </summary>
        /// <param name="scale">Initial frequency. Think rolling hills (low scale) vs jagged mountains (high scale).</param>
        /// <param name="seed">Fixes the randomness.</param>
        public LayeredNoise(float scale, int seed) : this(1, 1f, 1f, scale, seed)
        {

        }

        /// <summary>
        /// Get a random value between 0 and 1. 
        /// </summary>
        public float GetHeight(float x, float y)
        {
            float height = 0f;
            for (int i = 0; i < amplitudes.Length; i++)
            {
                Vector2 sample = offsets[i] + (new Vector2(x, y)) * frequencies[i] / scale;
                float perlinValue = TransformRange(sample);
                height += perlinValue * amplitudes[i];
            }
            return Mathf.InverseLerp(-1f, 1f, height);
        }

        // Transforms range from (0,1) to (-1,1) for both points in vector.
        float TransformRange(Vector2 sample)
        {
            return Mathf.PerlinNoise(sample.x, sample.y) * 2 - 1;
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