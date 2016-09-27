namespace CaveGeneration.MapGeneration
{
    static class Tooltips
    {
        public const string MAP_WALL_HEIGHT = "Height of walls before height maps are applied. Minimum of 1.";
        public const string MAP_LENGTH = "Number of units across in the x-axis occupied by the map.";
        public const string MAP_WIDTH = "Number of units across in the z-axis occupied by the map.";
        public const string MAP_DENSITY =
            "Initial proportion of walls in the map, from 0 to 1. Note that the final proportion will likely be " +
            "very different due to the various processing steps. Experiment to achieve desired proportion.";
        public const string MAP_FLOOR_EXPANSION = "Expand floor regions in every direction by given quantity.";
        public const string MAP_SEED =
            "The seed uniquely identifies which map gets generated. If using random seed, getting the seed will " +
            "set its value to some random seed.";
        public const string MAP_USE_RANDOM_SEED =
            "If set to true, a random map will be generated. If false, the seed property will be used to specify the map";
        public const string MAP_BORDER_SIZE = "The width of extra boundary around the map.";
        public const string MAP_SQUARE_SIZE =
            "How many game units each tile in the map should occupy. By default, each tile occupies 1 game unit " +
            "so that a 100 by 100 map takes 100 by 100 game units. Does not affect walls.";
        public const string MAP_MIN_WALL_SIZE =
            "Contiguous sections of wall with a tile count below this number will be removed (turned " +
            "to floor tiles). Regardless of how large this number is, the component of wall attached to the boundary " +
            "will not be removed.";
        public const string MAP_MIN_FLOOR_SIZE =
            "Contiguous sections of floor with a tile count below this number will be removed (turned to wall tiles).";
    }
}