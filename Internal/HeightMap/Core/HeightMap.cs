using IHeightMap = CaveGeneration.MeshGeneration.IHeightMap;

namespace CaveGeneration.HeightMaps
{
    /// <summary>
    /// A generalization of perlin noise, this class allows the generation of continuously varying, random values
    /// designed with terrain in mind.
    /// </summary>
    sealed class HeightMap : IHeightMap
    {
        public int BaseHeight { get; private set; }
        public bool IsSimple { get; private set; }
        public float MaxHeight { get; private set; }
        LayeredNoise noise;

        public HeightMap(LayeredNoise noise, int baseHeight, float maxHeight)
        {
            this.noise = noise;
            BaseHeight = baseHeight;
            MaxHeight = maxHeight;
            IsSimple = false;
        }

        /// <summary>
        /// Analogous to Mathf.PerlinNoise, this method returns a height value parametrized by a pair of floats.
        /// </summary>
        /// <returns>A float between 0 and max height</returns>
        public float GetHeight(float x, float y)
        {
            return MaxHeight * noise.GetHeight(x, y);
        }
    } 
}
