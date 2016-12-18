/* Fairly straightforward data container representing a possible connection between two rooms in a map. Holds
 * information about the two tiles being connected, the distance between them, and the index corresponding to the 
 * rooms between them. 
 * 
 * In order to keep size in the 16 byte soft limit for structs, the room indices are tracked by 
 * unsigned shorts instead of ints, which limits their use to cases where there are fewer than 2^16 rooms. 
 * This is not a concern in practice, however - testing shows that in the worst case, even an impractical 
 * 4000 by 4000 map with settings tailored to maximize the number of rooms does not get anywhere near this number.
 * An alternative would be to compute the distance from the tiles rather than store it, but that would hurt performance
 * in an unintuitive way. The other obvious alternative would be to make this object a class instead of a struct. 
 * But given that it's an immutable data container, value semantics make more sense, and given how many need to be
 * created, the memory management implications are not trivial. 
 */

namespace CaveGeneration.MapGeneration.Connectivity
{
    /// <summary>
    /// Represents a possible connection in the map.
    /// </summary>
    struct ConnectionInfo
    {
        public readonly Coord tileA;
        public readonly Coord tileB;
        public readonly ushort roomIndexA;
        public readonly ushort roomIndexB;
        public readonly float distance;

        public ConnectionInfo(Coord tileA, Coord tileB, int roomIndexA, int roomIndexB, float distance)
        {
            this.tileA = tileA;
            this.tileB = tileB;
            this.roomIndexA = (ushort)roomIndexA;
            this.roomIndexB = (ushort)roomIndexB;
            this.distance = distance;
        }
    } 
}
