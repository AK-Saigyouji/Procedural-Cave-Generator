public interface IHeightMap {
    int BaseHeight { get; }
    bool IsSimple { get; }
    float GetHeight(float x, float y);
}