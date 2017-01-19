using System;
using UnityEngine;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Convenience class for passing parameters for the map generator across interfaces. Properties can be set through
    /// code or inspector.
    /// </summary>
    [Serializable]
    public sealed class MapParameters
    {
        [Tooltip(Tooltips.MAP_LENGTH)]
        [SerializeField] int length;
        /// <summary>
        /// Must be between 5 and 32767.
        /// </summary>
        public int Length
        {
            get { return length; }
            set { SetLength(value); }
        }

        [Tooltip(Tooltips.MAP_WIDTH)]
        [SerializeField] int width;
        /// <summary>
        /// Must be between 5 and 32767.
        /// </summary>
        public int Width
        {
            get { return width; }
            set { SetWidth(value); }
        }

        [Tooltip(Tooltips.MAP_DENSITY)]
        [Range(MINIMUM_MAP_DENSITY, MAXIMUM_MAP_DENSITY)]
        [SerializeField] float initialMapDensity;
        /// <summary>
        /// Initial map density, from 0 (all floors) to 1 (all walls). Note that the final density will differ, based
        /// on what other processing steps are used. Less than 0.4 or greater than 0.6 will likely result in a map
        /// consisting entirely of floors or walls.
        /// </summary>
        public float InitialDensity
        {
            get { return initialMapDensity; }
            set { SetMapDensity(value); }
        }

        [Tooltip(Tooltips.MAP_SEED)]
        [SerializeField] int seed;
        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        [Tooltip(Tooltips.MAP_BORDER_SIZE)]
        [SerializeField] int borderSize;
        /// <summary>
        /// Adds a border of walls this thick on each side of the map. Must be between 0 and 1000.
        /// </summary>
        public int BorderSize
        {
            get { return borderSize; }
            set { SetBorderSize(value); }
        }

        [Tooltip(Tooltips.MAP_MIN_WALL_SIZE)]
        [SerializeField] int minWallSize;
        /// <summary>
        /// Prunes regions of wall with fewer than this many tiles. Min value of 0.
        /// </summary>
        public int MinWallSize
        {
            get { return minWallSize; }
            set { minWallSize = value; }
        }

        [Tooltip(Tooltips.MAP_MIN_FLOOR_SIZE)]
        [SerializeField] int minFloorSize;
        /// <summary>
        /// Prunes regions of floor with fewer than this many tiles. Min value of 0.
        /// </summary>
        public int MinFloorSize
        {
            get { return minFloorSize; }
            set { minFloorSize = value; }
        }

        const int MINIMUM_LENGTH = 5;
        const int DEFAULT_LENGTH = 75;
        const int MAXIMUM_LENGTH = short.MaxValue;

        const int MINIMUM_WIDTH = 5;
        const int DEFAULT_WIDTH = 75;
        const int MAXIMUM_WIDTH = short.MaxValue;

        const int MINIMUM_BORDER_SIZE = 0;
        const int DEFAULT_BORDER_SIZE = 0;
        const int MAXIMUM_BORDER_SIZE = 1000;

        const float MINIMUM_MAP_DENSITY = 0f;
        const float DEFAULT_MAP_DENSITY = 0.5f;
        const float MAXIMUM_MAP_DENSITY = 1f;

        const int MINIMUM_FLOOR_THRESHOLD = 0;
        const int DEFAULT_FLOOR_THRESHOLD = 50;

        const int MINIMUM_WALL_THRESHOLD = 0;
        const int DEFAULT_WALL_THRESHOLD = 50;

        public MapParameters()
        {
            Reset();
        }

        /// <summary>
        /// Create a copy of the map parameters. 
        /// </summary>
        public MapParameters Clone()
        {
            var newParameters = (MapParameters)MemberwiseClone();
            return newParameters;
        }

        internal void OnValidate()
        {
            length            = Mathf.Clamp(length, MINIMUM_LENGTH, MAXIMUM_LENGTH);
            width             = Mathf.Clamp(width, MINIMUM_WIDTH, MAXIMUM_WIDTH);
            initialMapDensity = Mathf.Clamp(initialMapDensity, MINIMUM_MAP_DENSITY, MAXIMUM_MAP_DENSITY);
            borderSize        = Mathf.Clamp(borderSize, MINIMUM_BORDER_SIZE, MAXIMUM_BORDER_SIZE);
        }

        void Reset()
        {
            length            = DEFAULT_LENGTH;
            width             = DEFAULT_WIDTH;
            initialMapDensity = DEFAULT_MAP_DENSITY;
            borderSize        = DEFAULT_BORDER_SIZE;
            minWallSize       = DEFAULT_WALL_THRESHOLD;
            minFloorSize      = DEFAULT_FLOOR_THRESHOLD;
        }

        void SetLength(int value)
        {
            SetParameter(ref length, value, MINIMUM_LENGTH, MAXIMUM_LENGTH);
        }

        void SetWidth(int value)
        {
            SetParameter(ref width, value, MINIMUM_WIDTH, MAXIMUM_WIDTH);
        }

        void SetBorderSize(int value)
        {
            SetParameter(ref borderSize, value, MINIMUM_BORDER_SIZE, MAXIMUM_BORDER_SIZE);
        }

        void SetMapDensity(float value)
        {
            SetParameter(ref initialMapDensity, value, MINIMUM_MAP_DENSITY, MAXIMUM_MAP_DENSITY);
        }

        void SetParameter<T>(ref T parameter, T value, T minimum, T maximum) where T: IComparable<T>
        {
            bool tooSmall = value.CompareTo(minimum) == -1;
            bool tooBig = value.CompareTo(maximum) == 1;
            if (tooSmall)
            {
                const string smallErrorFormat = "Parameter {0} must be at least {1}.";
                throw new ArgumentOutOfRangeException(string.Format(smallErrorFormat, parameter, minimum));
            }
            else if (tooBig)
            {
                const string largeErrorFormat = "Parameter {0} must be at most {1}.";
                throw new ArgumentOutOfRangeException(string.Format(largeErrorFormat, parameter, maximum));
            }
            else
            {
                parameter = value;
            }
        }
    } 
}
