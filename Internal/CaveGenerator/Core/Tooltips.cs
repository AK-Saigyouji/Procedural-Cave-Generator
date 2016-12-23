/* In the future, these should ideally be moved to a resource file rather than kept in code.*/

namespace CaveGeneration
{
    static class Tooltips
    {
        public const string CAVE_GEN_DEBUG_MODE = 
            "Disables multithreading for profiling and debugging purposes.";

        public const string CAVE_GEN_SCALE = 
            "How many game units each 'tile' in the cave should occupy.";

        public const string HEIGHT_MAP_CONSTANT =
            "Should the height of this component be constant?";

        public const string HEIGHT_MAP_MIN_HEIGHT =
            "Minimum y-value for the height map.";

        public const string HEIGHT_MAP_MAX_HEIGHT = 
            "Maximum y-value for the height map.";

        public const string HEIGHT_MAP_SMOOTHNESS = 
            "How smooth the height map is initially - think rolling hills vs jagged mountains.";

        public const string HEIGHT_MAP_NUM_LAYERS =
            "The number of height maps to stack onto eachother, each rockier and with smaller contribution to add"
            + " finer variations. Set the next two properties to control the contribution of each layer.";

        public const string HEIGHT_MAP_CONTRIBUTION_MULT =
            "What proportion of each subsequent layer to use. e.g. 0.5 means each layer provides half the contribution"
            + " of the previous layer.";

        public const string HEIGHT_MAP_COMPRESSION_MULT = 
            "How much rockier (less smooth) each layer becomes.";
    } 
}