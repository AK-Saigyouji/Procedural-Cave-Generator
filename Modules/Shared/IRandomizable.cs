namespace CaveGeneration.Modules
{
    /// <summary>
    /// Interface for modules affected by a seed value to determine randomness.
    /// </summary>
    public interface IRandomizable
    {
        int Seed { set; }
    }
}