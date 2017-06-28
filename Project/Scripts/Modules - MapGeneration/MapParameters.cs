using System;
using UnityEngine;

namespace AKSaigyouji.Modules.MapGeneration
{
    /// <summary>
    /// Convenience class that groups up the properties for a cellular automata map generator and all the defaults and
    /// validation logic.
    /// </summary>
    [Serializable]
    public sealed class MapParameters
    {
        #region PARAMETERS

        [Tooltip(LENGTH_TOOLTIP)]
        [SerializeField] int length;
        /// <summary>
        /// Must be between 5 and 32767.
        /// </summary>
        public int Length
        {
            get { return length; }
            set { SetLength(value); }
        }

        [Tooltip(WIDTH_TOOLTIP)]
        [SerializeField] int width;
        /// <summary>
        /// Must be between 5 and 32767.
        /// </summary>
        public int Width
        {
            get { return width; }
            set { SetWidth(value); }
        }

        [Tooltip(DENSITY_TOOLTIP)]
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

        [Tooltip(EXPAND_TUNNELS_TOOLTIP)]
        [SerializeField] bool expandTunnels;
        public bool ExpandTunnels
        {
            get { return expandTunnels; }
            set { expandTunnels = value; }
        }

        [Tooltip(BORDER_SIZE_TOOLTIP)]
        [SerializeField] int borderSize;
        /// <summary>
        /// Adds a border of walls this thick on each side of the map. Must be between 0 and 1000.
        /// </summary>
        public int BorderSize
        {
            get { return borderSize; }
            set { SetBorderSize(value); }
        }

        [Tooltip(MIN_WALL_SIZE_TOOLTIP)]
        [SerializeField] int minWallSize;
        /// <summary>
        /// Prunes regions of wall with fewer than this many tiles. Min value of 0.
        /// </summary>
        public int MinWallSize
        {
            get { return minWallSize; }
            set { minWallSize = value; }
        }

        [Tooltip(MIN_FLOOR_SIZE_TOOLTIP)]
        [SerializeField] int minFloorSize;
        /// <summary>
        /// Prunes regions of floor with fewer than this many tiles. Min value of 0.
        /// </summary>
        public int MinFloorSize
        {
            get { return minFloorSize; }
            set { minFloorSize = value; }
        }
        #endregion

        #region TOOLTIPS
        const string LENGTH_TOOLTIP =
            "Number of units across in the x-axis occupied by the map.";

        const string WIDTH_TOOLTIP =
            "Number of units across in the z-axis occupied by the map.";

        const string EXPAND_TUNNELS_TOOLTIP =
            "Expands tunnels to a minimum width of 2. Useful for ensuring colliders don't get stuck in 1-unit passages.";

        const string DENSITY_TOOLTIP = 
            "Initial proportion of walls in the map, from 0 to 1. Note that the final proportion will likely be"
        + " very different due to the various processing steps. Experiment to achieve desired proportion.";

        const string BORDER_SIZE_TOOLTIP =
            "The width of extra boundary around the map. Each unit of border adds 2 units to final length and width.";

        const string MIN_WALL_SIZE_TOOLTIP =
            "Contiguous sections of wall with a tile count below this number will be removed (turned to floor tiles).";

        const string MIN_FLOOR_SIZE_TOOLTIP = 
            "Contiguous sections of floor with a tile count below this number will be removed (turned to wall tiles).";
        #endregion

        #region VALUES
        const int MINIMUM_LENGTH = 5;
        const int DEFAULT_LENGTH = 45;
        const int MAXIMUM_LENGTH = short.MaxValue;

        const int MINIMUM_WIDTH = 5;
        const int DEFAULT_WIDTH = 45;
        const int MAXIMUM_WIDTH = short.MaxValue;

        const int MINIMUM_BORDER_SIZE = 0;
        const int DEFAULT_BORDER_SIZE = 1;
        const int MAXIMUM_BORDER_SIZE = ushort.MaxValue;

        const int MINIMUM_PASSAGE_RADIUS = 0;
        const int DEFAULT_PASSAGE_RADIUS = 0;
        const int MAXIMUM_PASSAGE_RADIUS = int.MaxValue;

        const bool DEFAULT_EXPAND_TUNNELS = true;

        const float MINIMUM_MAP_DENSITY = 0f;
        const float DEFAULT_MAP_DENSITY = 0.5f;
        const float MAXIMUM_MAP_DENSITY = 1f;

        const int MINIMUM_FLOOR_THRESHOLD = 0;
        const int DEFAULT_FLOOR_THRESHOLD = 25;

        const int MINIMUM_WALL_THRESHOLD = 0;
        const int DEFAULT_WALL_THRESHOLD = 25;
        #endregion

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
            minWallSize       = Mathf.Max(minWallSize, MINIMUM_WALL_THRESHOLD);
            minFloorSize      = Mathf.Max(minFloorSize, MINIMUM_FLOOR_THRESHOLD);
        }

        void Reset()
        {
            length            = DEFAULT_LENGTH;
            width             = DEFAULT_WIDTH;
            initialMapDensity = DEFAULT_MAP_DENSITY;
            expandTunnels     = DEFAULT_EXPAND_TUNNELS;
            borderSize        = DEFAULT_BORDER_SIZE;
            minWallSize       = DEFAULT_WALL_THRESHOLD;
            minFloorSize      = DEFAULT_FLOOR_THRESHOLD;
        }

        void SetLength(int value)
        {
            ValidateArgument(value, MINIMUM_LENGTH, MAXIMUM_LENGTH, "Length");
            length = value;
        }

        void SetWidth(int value)
        {
            ValidateArgument(value, MINIMUM_WIDTH, MAXIMUM_WIDTH, "Width");
            width = value;
        }

        void SetBorderSize(int value)
        {
            ValidateArgument(value, MINIMUM_BORDER_SIZE, MAXIMUM_BORDER_SIZE, "Border");
            borderSize = value;
        }

        void SetMapDensity(float value)
        {
            ValidateArgument(value, MINIMUM_MAP_DENSITY, MAXIMUM_MAP_DENSITY, "Map density");
            initialMapDensity = value;
        }

        void ValidateArgument<T>(T value, T minimum, T maximum, string name) where T : IComparable<T>
        {
            bool tooSmall = value.CompareTo(minimum) == -1;
            bool tooBig = value.CompareTo(maximum) == 1;
            if (tooSmall)
            {
                const string smallErrorFormat = "{0} must be at least {1}.";
                throw new ArgumentOutOfRangeException(string.Format(smallErrorFormat, name, minimum));
            }
            else if (tooBig)
            {
                const string largeErrorFormat = "{0} must be at most {1}.";
                throw new ArgumentOutOfRangeException(string.Format(largeErrorFormat, name, maximum));
            }
        }
    } 
}
