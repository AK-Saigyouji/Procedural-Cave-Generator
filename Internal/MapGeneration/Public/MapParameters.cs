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
        public int Length
        {
            get { return length; }
            set { SetLength(value); }
        }

        [Tooltip(Tooltips.MAP_WIDTH)]
        [SerializeField] int width;
        public int Width
        {
            get { return width; }
            set { SetWidth(value); }
        }

        [Tooltip(Tooltips.MAP_DENSITY)]
        [Range(MINIMUM_MAP_DENSITY, MAXIMUM_MAP_DENSITY)]
        [SerializeField] float initialMapDensity;
        public float InitialDensity
        {
            get { return initialMapDensity; }
            set { SetMapDensity(value); }
        }

        [Tooltip(Tooltips.MAP_FLOOR_EXPANSION)]
        [SerializeField] int floorExpansion;
        public int FloorExpansion
        {
            get { return floorExpansion; }
            set { SetFloorExpansion(value); }
        }

        [Tooltip(Tooltips.MAP_SEED)]
        [SerializeField] int seed;
        public int Seed
        {
            get { return useRandomSeed ? CreateRandomSeed() : seed; }
            set { seed = value; }
        }

        [Tooltip(Tooltips.MAP_USE_RANDOM_SEED)]
        [SerializeField] bool useRandomSeed;
        public bool UseRandomSeed
        {
            get { return useRandomSeed; }
            set { useRandomSeed = value; }
        }

        [Tooltip(Tooltips.MAP_BORDER_SIZE)]
        [SerializeField] int borderSize;
        public int BorderSize
        {
            get { return borderSize; }
            set { SetBorderSize(value); }
        }

        [Tooltip(Tooltips.MAP_MIN_WALL_SIZE)]
        [SerializeField] int minWallSize;
        public int MinWallSize
        {
            get { return minWallSize; }
            set { minWallSize = value; }
        }

        [Tooltip(Tooltips.MAP_MIN_FLOOR_SIZE)]
        [SerializeField] int minFloorSize;
        public int MinFloorSize
        {
            get { return minFloorSize; }
            set { minFloorSize = value; }
        }

        const int MINIMUM_LENGTH          = 5;
        const int MINIMUM_WIDTH           = 5;
        const int MINIMUM_BORDER_SIZE     = 0;
        const int MINIMUM_FLOOR_EXPANSION = 0;
        const float MINIMUM_MAP_DENSITY   = 0f;
        const float MAXIMUM_MAP_DENSITY   = 1f;

        const float DEFAULT_DENSITY       = 0.5f;
        const bool DEFAULT_SEED_STATUS    = true;
        const int DEFAULT_SEED = 0;
        const int DEFAULT_LENGTH          = 75;
        const int DEFAULT_WIDTH           = 75;
        const int DEFAULT_BORDER_SIZE     = 0;
        const int DEFAULT_FLOOR_EXPANSION = 0;
        const int DEFAULT_WALL_THRESHOLD  = 50;
        const int DEFAULT_FLOOR_THRESHOLD = 50;
        const int DEFAULT_WALL_HEIGHT     = 3;

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

        void Reset()
        {
            length            = DEFAULT_LENGTH;
            width             = DEFAULT_WIDTH;
            initialMapDensity = DEFAULT_DENSITY;
            floorExpansion    = DEFAULT_FLOOR_EXPANSION;
            useRandomSeed     = DEFAULT_SEED_STATUS;
            seed              = DEFAULT_SEED;
            borderSize        = DEFAULT_BORDER_SIZE;
            minWallSize       = DEFAULT_WALL_THRESHOLD;
            minFloorSize      = DEFAULT_FLOOR_THRESHOLD;
        }

        public void OnValidate()
        {
            length            = Mathf.Max(length, MINIMUM_WIDTH);
            width             = Mathf.Max(width, MINIMUM_WIDTH);
            floorExpansion    = Mathf.Max(floorExpansion, MINIMUM_FLOOR_EXPANSION);
            borderSize        = Mathf.Max(borderSize, MINIMUM_BORDER_SIZE);
            initialMapDensity = Mathf.Clamp(initialMapDensity, MINIMUM_MAP_DENSITY, MAXIMUM_MAP_DENSITY);
        }

        int CreateRandomSeed()
        {
            return Guid.NewGuid().GetHashCode();
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

        void SetFloorExpansion(int value)
        {
            SetParameter(ref floorExpansion, value, MINIMUM_FLOOR_EXPANSION, int.MaxValue);
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
