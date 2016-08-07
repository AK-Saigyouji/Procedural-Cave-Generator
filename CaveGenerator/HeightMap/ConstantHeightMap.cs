namespace CaveGeneration
{
    public class ConstantHeightMap : IHeightMap
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
