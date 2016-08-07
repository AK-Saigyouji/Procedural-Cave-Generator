namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Responsible for building up a meshdata object in a threadsafe way.
    /// </summary>
    interface IMeshBuilder
    {
        MeshData Build();
    } 
}