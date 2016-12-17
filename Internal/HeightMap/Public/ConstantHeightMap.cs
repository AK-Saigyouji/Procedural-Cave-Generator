/* When generating caves, height maps are optional. This originally resulted in messy code as a result of passing nulls across
 * interfaces. This class was created to address that issue by representing the absence of a height map. It can be consumed 
 * like a normal height map, returning a constant number; */

namespace CaveGeneration.HeightMaps
{
    sealed class ConstantHeightMap : MeshGeneration.IHeightMap
    {
        public float MinHeight { get { return height; } }
        public float MaxHeight { get { return height; } }

        float height;

        /// <summary>
        /// A height map that has the same value for all coordinates..
        /// </summary>
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
