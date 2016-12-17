namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// A height map gives a height value for each pair of floats. BaseHeight is a constant offset, and GetHeight
    /// provides the variation in the form of a float between 0 and MaxHeight for each pair of input floats.
    /// </summary>
    public interface IHeightMap
    {
        /// <summary>
        /// Minimum possible value returned by get height.
        /// </summary>
        float MinHeight { get; }

        /// <summary>
        /// Highest possible value returned by get height.
        /// </summary>
        float MaxHeight { get; }

        /// <summary>
        /// Return a height value parametrized by a pair of floats. Identical input will produce identical output.
        /// </summary>
        /// <returns>Float between MinHeight and MaxHeight.</returns>
        float GetHeight(float x, float y);
    }
}