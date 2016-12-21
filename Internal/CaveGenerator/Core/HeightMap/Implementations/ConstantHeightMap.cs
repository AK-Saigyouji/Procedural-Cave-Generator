namespace CaveGeneration
{
    /// <summary>
    /// A height map that has the same value for all coordinates.
    /// </summary>
    sealed class ConstantHeightMap : MeshGeneration.IHeightMap
    {
        public float MinHeight { get { return height; } }
        public float MaxHeight { get { return height; } }

        float height;

        public ConstantHeightMap(float minHeight)
        {
            height = minHeight;
        }

        public float GetHeight(float x, float y)
        {
            return height;
        }
    }
}
