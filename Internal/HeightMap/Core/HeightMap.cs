namespace CaveGeneration.HeightMaps
{
    /// <summary>
    /// A generalization of perlin noise, this class allows the generation of continuously varying, random values
    /// designed with terrain in mind.
    /// </summary>
    sealed class HeightMap : MeshGeneration.IHeightMap
    {
        public float MaxHeight { get { return maxHeight; } }
        public float MinHeight { get { return baseHeight; } }

        float baseHeight;
        float maxHeight;

        float scale;

        LayeredNoise noise;

        public HeightMap(LayeredNoise noise, float minHeight, float maxHeight)
        {
            if (maxHeight < minHeight)
                throw new System.ArgumentException("Max height can't be less than min height.");
            this.noise = noise;
            baseHeight = minHeight;
            this.maxHeight = maxHeight;
            scale = maxHeight - minHeight;
        }

        /// <summary>
        /// Analogous to Mathf.PerlinNoise, this method returns a height value parametrized by a pair of floats.
        /// </summary>
        public float GetHeight(float x, float y)
        {
            return baseHeight + scale * noise.GetHeight(x, y);
        }
    } 
}
