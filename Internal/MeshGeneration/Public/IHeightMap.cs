namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// A height map gives a height value for each pair of floats. BaseHeight is a constant offset, and GetHeight
    /// provides the variation in the form of a float between 0 and MaxHeight for each pair of input floats.
    /// </summary>
    public interface IHeightMap
    {
        /// <summary>
        /// A constant offset for the height map. 
        /// </summary>
        int BaseHeight { get; }

        /// <summary>
        /// If true, get height will return 0 for each pair of values, and the only useful information in the class is
        /// held by the base height.
        /// </summary>
        bool IsSimple { get; }

        /// <summary>
        /// Highest possible value returned by get height.
        /// </summary>
        float MaxHeight { get; }

        /// <summary>
        /// Return a height value parametrized by a pair of floats. Identical input will produce identical output.
        /// </summary>
        /// <returns>A float between 0 and max height.</returns>
        float GetHeight(float x, float y);
    }
}