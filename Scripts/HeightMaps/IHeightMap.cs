namespace AKSaigyouji.HeightMaps
{
    /// <summary>
    /// Height maps provide a float y for each coordinate pair x,z of floats. MinHeight and MaxHeight
    /// indicate the lower and upper bounds for the height values. 
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