namespace CaveGeneration
{
    /// <summary>
    /// A generalization of perlin noise, this class allows the generation of continuously varying, random values
    /// designed with terrain in mind.
    /// </summary>
    sealed class PerlinHeightMap : MeshGeneration.IHeightMap
    {
        public float MaxHeight { get { return maxHeight; } }
        public float MinHeight { get { return minHeight; } }

        readonly float minHeight;
        readonly float maxHeight;
        readonly float heightVariation;

        readonly LayeredNoise noise;

        public float GetHeight(float x, float y)
        {
            return minHeight + heightVariation * noise.GetHeight(x, y);
        }

        public PerlinHeightMap(LayeredNoise noise, float minHeight, float maxHeight)
        {
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            this.noise = noise;
            heightVariation = maxHeight - minHeight;
        }
    } 
}
