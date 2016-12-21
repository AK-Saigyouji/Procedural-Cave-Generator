namespace CaveGeneration.MapGeneration
{
    static class Tooltips
    {
        public const string MAP_LENGTH =
            "Number of units across in the x-axis occupied by the map.";

        public const string MAP_WIDTH = 
            "Number of units across in the z-axis occupied by the map.";

        public const string MAP_DENSITY =
            "Initial proportion of walls in the map, from 0 to 1. Note that the final proportion will likely be " +
            "very different due to the various processing steps. Experiment to achieve desired proportion.";

        public const string MAP_FLOOR_EXPANSION = 
            "Expand floor regions in every direction by given quantity.";

        public const string MAP_SEED =
            "The seed uniquely identifies which map gets generated. If using random seed, getting the seed will " +
            "randomly reset its value.";

        public const string MAP_USE_RANDOM_SEED =
            "If set to true, a random map will be generated. If false, the seed property will be used to specify the map." +
            "While true, the seed property will return random values. Disable before attemping to read or set a specific seed.";

        public const string MAP_BORDER_SIZE = 
            "The width of extra boundary around the map.";

        public const string MAP_MIN_WALL_SIZE =
            "Contiguous sections of wall with a tile count below this number will be removed (turned " +
            "to floor tiles). Regardless of how large this number is, the component of wall attached to the boundary " +
            "will not be removed.";

        public const string MAP_MIN_FLOOR_SIZE =
            "Contiguous sections of floor with a tile count below this number will be removed (turned to wall tiles).";
    }
}