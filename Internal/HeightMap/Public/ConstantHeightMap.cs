/* When generating caves, height maps are optional. This originally resulted in messy code as a result of passing nulls across
 * interfaces. This class was created to address that issue by representing the absence of a height map. It can be consumed 
 * like a normal height map (in which case it returns a constant value) but also identifies 
 * itself through the exposed IsSimple property, allowing for a more optimized approach.*/

using IHeightMap = CaveGeneration.MeshGeneration.IHeightMap;

namespace CaveGeneration.HeightMaps
{
    sealed class ConstantHeightMap : IHeightMap
    {
        public int BaseHeight { get; private set; }
        public bool IsSimple { get; private set; }
        public float MaxHeight { get { return BaseHeight; } }

        /// <summary>
        /// A trivial height map that has a constant base height with no variation provided by get height.
        /// </summary>
        public ConstantHeightMap(int baseHeight)
        {
            BaseHeight = baseHeight;
            IsSimple = true;
        }

        public float GetHeight(float x, float y)
        {
            return 0;
        }
    }
}
