﻿using UnityEngine;
using System.Collections;

namespace CaveGeneration
{
    [System.Serializable]
    /// <summary>
    /// Convenience class for passing parameters for the map generator across interfaces.
    /// </summary>
    public class MapParameters
    {
        [Tooltip("Number of units across in the x-axis occupied by the map.")]
        [SerializeField]
        int length;
        public int Length
        {
            get { return length; }
            set { SetLength(value); }
        }

        [Tooltip("Number of units across in the z-axis occupied by the map.")]
        [SerializeField]
        int width;
        public int Width
        {
            get { return width; }
            set { SetWidth(value); }
        }

        [Tooltip("Initial proportion of walls in the map, from 0 to 1. Note that the final proportion will be " +
        "different. In particular, an initial map density of below 0.5 will end up smaller, while an initial density " +
        "of above 0.5 will end up larger.")]
        [SerializeField]
        float initialMapDensity;
        public float InitialMapDensity
        {
            get { return initialMapDensity; }
            set { SetMapDensity(value); }
        }

        [Tooltip("The seed uniquely identifies which map gets generated, if useRandomSeed is set to false.")]
        [SerializeField]
        string seed;
        public string Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        [Tooltip("If set to true, a random map will be generated. If false, the seed property will be used to specify " + 
        "the map")]
        [SerializeField]
        bool useRandomSeed;
        public bool UseRandomSeed
        {
            get { return useRandomSeed; }
            set { useRandomSeed = value; }
        }

        [Tooltip("The width of extra boundary around the map.")]
        [SerializeField]
        int borderSize;
        public int BorderSize
        {
            get { return borderSize; }
            set { SetBorderSize(value); }
        }

        [Tooltip("How many game units each tile in the map should occupy. This is a cheap (in terms of memory " +
        "and computation) way to increase the size of the map. Can be used to ensure that corridors are large " +
        "enough to accomodate large game objects.")]
        [SerializeField]
        int squareSize;
        public int SquareSize
        {
            get { return squareSize; }
            set { SetSquareSize(value); }
        }

        [Tooltip("Contiguous sections of wall with a tile count below this number will be removed (turned " + 
        "to floor tiles). Regardless of how large this number is, the component of wall attached to the boundary " +
        "will not be removed.")]
        [SerializeField]
        int minWallSize;
        public int MinWallSize
        {
            get { return minWallSize; }
            set { minWallSize = value; }
        }

        [Tooltip("Contiguous sections of floor with a tile count below this number will be removed (turned to wall tiles).")]
        [SerializeField]
        int minFloorSize;
        public int MinFloorSize
        {
            get { return minFloorSize; }
            set { minFloorSize = value; }
        }

        const int MINIMUM_LENGTH = 5;
        const int MINIMUM_WIDTH = 5;
        const int MINIMUM_BORDER_SIZE = 0;
        const int MINIMUM_SQUARE_SIZE = 1;
        const float MINIMUM_MAP_DENSITY = 0.4f;
        const float MAXIMUM_MAP_DENSITY = 0.6f;

        const int DEFAULT_LENGTH = 75;
        const int DEFAULT_WIDTH = 75;
        const float DEFAULT_DENSITY = 0.5f;
        const bool DEFAULT_SEED_STATUS = true;
        const int DEFAULT_BORDER_SIZE = 0;
        const int DEFAULT_SQUARE_SIZE = 1;
        const int DEFAULT_WALL_THRESHOLD = 50;
        const int DEFAULT_FLOOR_THRESHOLD = 50;

        public void Reset()
        {
            length = DEFAULT_LENGTH;
            width = DEFAULT_WIDTH;
            initialMapDensity = DEFAULT_DENSITY;
            useRandomSeed = DEFAULT_SEED_STATUS;
            borderSize = DEFAULT_BORDER_SIZE;
            squareSize = DEFAULT_SQUARE_SIZE;
            minWallSize = DEFAULT_WALL_THRESHOLD;
            minFloorSize = DEFAULT_FLOOR_THRESHOLD;
        }

        public void OnValidate()
        {
            if (length < MINIMUM_LENGTH)
            {
                length = MINIMUM_LENGTH;
            }
            if (width < MINIMUM_WIDTH)
            {
                width = MINIMUM_WIDTH;
            }
            if (squareSize < MINIMUM_SQUARE_SIZE)
            {
                squareSize = MINIMUM_SQUARE_SIZE;
            }
            if (borderSize < MINIMUM_BORDER_SIZE)
            {
                borderSize = MINIMUM_BORDER_SIZE;
            }
        }

        void SetLength(int value)
        {
            SetParameter(ref length, value, MINIMUM_LENGTH, int.MaxValue);
        }

        void SetWidth(int value)
        {
            SetParameter(ref width, value, MINIMUM_WIDTH, int.MaxValue);
        }

        void SetBorderSize(int value)
        {
            SetParameter(ref borderSize, value, MINIMUM_BORDER_SIZE, int.MaxValue);
        }

        void SetSquareSize(int value)
        {
            SetParameter(ref squareSize, value, MINIMUM_SQUARE_SIZE, int.MaxValue);
        }

        void SetMapDensity(float value)
        {
            SetParameter(ref initialMapDensity, value, MINIMUM_MAP_DENSITY, MAXIMUM_MAP_DENSITY);
        }

        void SetParameter<T>(ref T parameter, T value, T minimum, T maximum) where T: System.IComparable<T>
        {
            bool tooSmall = value.CompareTo(minimum) == -1;
            bool tooBig = value.CompareTo(maximum) == 1;
            if (tooSmall)
            {
                throw new System.ArgumentException("Parameter " + parameter + " must be at least " + minimum + ".");
            }
            else if (tooBig)
            {
                throw new System.ArgumentException("Parameter " + parameter + " must be less than " + maximum + ".");
            }
            else
            {
                parameter = value;
            }
        }


    } 
}
