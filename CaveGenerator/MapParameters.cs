using UnityEngine;
using System.Collections;

namespace CaveGeneration
{
    public class MapParameters
    {
        public int length { get; set; }
        public int width { get; set; }
        public float mapDensity { get; set; }
        public string seed { get; set; }
        public bool useRandomSeed { get; set; }
        public int borderSize { get; set; }
        public int squareSize { get; set; }

        readonly int MINIMUM_LENGTH = 5;
        readonly int MINIMUM_WIDTH = 5;
        readonly int MINIMUM_BORDERSIZE = 0;
        readonly int MINIMUM_SQUARESIZE = 1;

        public MapParameters(int length, int width, float mapDensity = 0.5f, string seed = "", bool useRandomSeed = true,
            int borderSize = 0, int squareSize = 1)
        {
            this.length = length;
            this.width = width;
            this.mapDensity = mapDensity;
            this.seed = seed;
            this.useRandomSeed = useRandomSeed;
            this.borderSize = borderSize;
            this.squareSize = squareSize;
        }

        /// <summary>
        /// Checks if the parameters are valid for the generation of a map, throwing exceptions otherwise.
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
