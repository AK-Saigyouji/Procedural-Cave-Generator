using System;

namespace AKSaigyouji.HeightMaps
{
    /// <summary>
    /// Provides methods for the creation of height maps. All build methods return an object implementing
    /// the IHeightMap interface.
    /// </summary>
    public static class HeightMapFactory
    {
        /// <summary>
        /// Build a heightmap with a constant height.
        /// </summary>
        public static IHeightMap BuildConstant(float height)
        {
            return new ConstantHeightMap(height);
        }

        /// <summary>
        /// Build a heightmap based on perlin noise.
        /// </summary>
        public static IHeightMap BuildPerlin(float minHeight, float maxHeight, float scale, int seed)
        {
            ThrowIfHeightsAreInvalid(minHeight, maxHeight);

            if (minHeight == maxHeight) // handle special case more efficiently
                return new ConstantHeightMap(minHeight);

            var noise = new LayeredNoise(scale, seed);
            return new PerlinHeightMap(noise, minHeight, maxHeight);
        }

        /// <summary>
        /// Build a heightmap using a custom function. Its output will be clamped between minHeight and maxHeight;
        /// </summary>
        public static IHeightMap BuildCustom(Func<float, float, float> heightFunction, float minHeight, float maxHeight)
        {
            if (heightFunction == null)
                throw new ArgumentNullException("heightFunction");

            ThrowIfHeightsAreInvalid(minHeight, maxHeight);

            return new CustomHeightMap(minHeight, maxHeight, heightFunction);
        }

        /// <summary>
        /// Build heightmap out of multiple layers of Perlin noise.
        /// </summary>
        public static IHeightMap BuildLayeredPerlin(LayeredNoiseParameters noise, int seed)
        {
            return BuildLayeredPerlin(
                minHeight: noise.MinHeight,
                maxHeight: noise.MaxHeight,
                scale: noise.Smoothness,
                seed: seed,
                numLayers: noise.NumLayers,
                amplitudeFactor: noise.ContributionMult,
                frequencyGrowth: noise.CompressionMult);
        }

        static IHeightMap BuildLayeredPerlin(float minHeight, float maxHeight, float scale, int seed,
            int numLayers, float amplitudeFactor, float frequencyGrowth)
        {
            ThrowIfHeightsAreInvalid(minHeight, maxHeight);

            if (minHeight == maxHeight || numLayers == 0)
                return new ConstantHeightMap(minHeight);

            var noise = new LayeredNoise(numLayers, amplitudeFactor, frequencyGrowth, scale, seed);
            return new PerlinHeightMap(noise, minHeight, maxHeight);
        }

        /// <summary>
        /// Throws ArgumentException if min exceeds max, validating the min and max height.
        /// </summary>
        static void ThrowIfHeightsAreInvalid(float min, float max)
        {
            if (min > max)
                throw new ArgumentException("Minimum height cannot exceed maximum height.");
        }
    } 
}