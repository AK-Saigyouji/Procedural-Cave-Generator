using UnityEngine;
using System.Collections;

namespace CaveGeneration
{
    /// <summary>
    /// Holds the data for the configuration of various generators.
    /// </summary>
    public class MapParameters
    {
        /// <summary>
        /// Number of units across in the x-axis occupied by the map. Must be at least 5.
        /// </summary>
        public int length { get; set; }

        /// <summary>
        /// Number of units across in the z-axis occupied by the map. Must be at least 5.
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// Initial proportion of walls in the map, from 0 to 1. Note that the final proportion will be different. 
        /// In particular, an initial map density of below 0.5 will end up smaller, while an initial density of above 
        /// 0.5 will end up larger. For this reason, values outside of the range 0.4 to 0.6 are not recommended.
        /// </summary>
        public float mapDensity { get; set; }

        /// <summary>
        /// The seed uniquely identifies which map gets generated, if useRandomSeed is set to false. 
        /// </summary>
        public string seed { get; set; }

        /// <summary>
        /// If set to true, a random map will be generated. If false, the seed property will be used to specify the 
        /// map. 
        /// </summary>
        public bool useRandomSeed { get; set; }

        /// <summary>
        /// The width of extra boundary around the map. Must be at least 0.
        /// </summary>
        public int borderSize { get; set; }

        /// <summary>
        /// How many game units each tile in the map should occupy. This is a cheap (in terms of memory and computation)
        /// way to increase the size of the map. Can be used to ensure that corridors are large enough to accomodate 
        /// large game objects. Must be at least 1.
        /// </summary>
        public int squareSize { get; set; }

        /// <summary>
        /// Contiguous sections of wall with a tile count below this number will be removed (turned to floor tiles). 
        /// Regardless of how large this number is, the component of wall attached to the boundary will not be removed.
        /// </summary>
        public int minWallSize { get; set; }

        /// <summary>
        /// Contiguous sections of floor with a tile count below this number will be removed (turned to wall tiles).
        /// </summary>
        public int minFloorSize { get; set; }

        // validation constants
        const int MINIMUM_LENGTH = 5;
        const int MINIMUM_WIDTH = 5;
        const int MINIMUM_BORDERSIZE = 0;
        const int MINIMUM_SQUARESIZE = 1;

        /// <summary>
        /// Configure a map by hand. 
        /// </summary>
        /// <param name="length">Number of units across in the x-axis occupied by the map. Must be at least 5.</param>
        /// <param name="width">Number of units across in the z-axis occupied by the map. Must be at least 5.</param>
        /// <param name="mapDensity">Initial proportion of walls in the map, from 0 to 1. Note that the final proportion 
        /// will be different. In particular, an initial map density of below 0.5 will end up smaller, while an initial 
        /// density of above 0.5 will end up larger. For this reason, values outside of the range 0.4 to 0.6 are not 
        /// recommended.</param>
        /// <param name="seed">The seed uniquely identifies which map gets generated, if useRandomSeed is set to false. </param>
        /// <param name="useRandomSeed">If set to true, a random map will be generated. If false, the seed property
        /// will be used to specify the map. </param>
        /// <param name="borderSize">The width of extra boundary around the map. Must be at least 0.</param>
        /// <param name="squareSize">How many game units each tile in the map should occupy. This is a cheap (in terms of 
        /// memory and computation) way to increase the size of the map. Can be used to ensure that corridors are large 
        /// enough to accomodate large game objects. Must be at least 1.</param>
        /// <param name="minWallSize">Contiguous sections of wall with a tile count below this number will be removed 
        /// (turned to floor tiles). Regardless of how large this number is, the component of wall attached to the 
        /// boundary will not be removed.</param>
        /// <param name="minFloorSize">Contiguous sections of floor with a tile count below this number will be removed 
        /// (turned to wall tiles).</param>
        public MapParameters(int length, int width, float mapDensity = 0.5f, string seed = "", bool useRandomSeed = true,
            int borderSize = 0, int squareSize = 1, int minWallSize = 50, int minFloorSize = 50)
        {
            this.length = length;
            this.width = width;
            this.mapDensity = mapDensity;
            this.seed = seed;
            this.useRandomSeed = useRandomSeed;
            this.borderSize = borderSize;
            this.squareSize = squareSize;
            this.minWallSize = minWallSize;
            this.minFloorSize = minFloorSize;
        }

        /// <summary>
        /// An example MapParameters object of size 100 by 100.
        /// </summary>
        public MapParameters Sample {
            get
            {
                return new MapParameters(
                    length: 100,
                    width: 100,
                    mapDensity: 0.5f,
                    useRandomSeed: true,
                    borderSize: 0,
                    squareSize: 1
                    );
            }
        }

        /// <summary>
        /// Checks if the parameters are valid for the generation of a map, throwing an argument exception otherwise.
        /// </summary>
        public void Validate()
        {
            if (length < MINIMUM_LENGTH)
                throw new System.ArgumentException("Length must be at least " + MINIMUM_LENGTH);
            if (width < MINIMUM_WIDTH)
                throw new System.ArgumentException("Width must be at least " + MINIMUM_WIDTH);
            if (borderSize < MINIMUM_BORDERSIZE)
                throw new System.ArgumentException("Border Size must be at least " + MINIMUM_BORDERSIZE);
            if (squareSize < MINIMUM_SQUARESIZE)
                throw new System.ArgumentException("Square Size must be at least " + MINIMUM_SQUARESIZE);
        }
    } 
}
