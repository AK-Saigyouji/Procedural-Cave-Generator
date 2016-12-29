/* This serves as a container for the configurable properties of the cave generator, and handles the related
 book-keeping regarding validation, reset logic, etc.
 
  Warning: changes to the names of properties in this class may break the cave generator's custom inspector.*/

using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;
using UnityEngine;

namespace CaveGeneration
{
    /// <summary>
    /// Holds a complete configuration for a CaveGenerator. 
    /// </summary>
    [System.Serializable]
    public sealed class CaveConfiguration
    {
        [SerializeField] CaveType caveType;
        [SerializeField] MapParameters mapParameters;

        [SerializeField] Material ceilingMaterial;
        [SerializeField] Material wallMaterial;
        [SerializeField] Material floorMaterial;

        [SerializeField] HeightMapProperties floorHeight;
        [SerializeField] HeightMapProperties ceilingHeight;

        [SerializeField, Tooltip(Tooltips.CAVE_GEN_SCALE)] int scale;
        [SerializeField, Tooltip(Tooltips.CAVE_GEN_DEBUG_MODE)] bool debugMode;

        // Auto-properties won't get serialized properly, so their backing fields are explicitly implemented.
        public CaveType CaveType { get { return caveType; }  set { caveType  = value; } }
        public int Scale         { get { return scale; }     set { scale     = value; } }
        public bool DebugMode    { get { return debugMode; } set { debugMode = value; } }

        public Material CeilingMaterial { get { return ceilingMaterial; } set { value = ceilingMaterial; } }
        public Material WallMaterial    { get { return wallMaterial; }    set { value = wallMaterial; } }
        public Material FloorMaterial   { get { return floorMaterial; }   set { value = floorMaterial; } }

        public MapParameters MapParameters
        {
            get { return mapParameters; }
            set
            {
                if (value == null) throw new System.ArgumentNullException();
                mapParameters = value.Clone();
            }
        }

        /// <summary>
        /// To configure the floor's height map, use the inspector. Alternatively, supply a height map
        /// to the CaveGenerator directly when generating the cave. 
        /// </summary>
        public IHeightMap FloorHeightMap
        {
            get { return floorHeight.ToHeightMap(); }
        }

        /// <summary>
        /// To configure the ceiling's height map, use the inspector. Alternatively, supply a height map
        /// to the CaveGenerator directly when generating the cave.
        /// </summary>
        public IHeightMap CeilingHeightMap
        {
            get { return ceilingHeight.ToHeightMap(); }
        }

        const int MIN_SCALE = 1;
        const int DEFAULT_SCALE = 1;
        const int DEFAULT_FLOOR_HEIGHT = 0;
        const int DEFAULT_CEILING_HEIGHT = 3;
        const bool DEFAULT_DEBUG_MODE = false;

        internal CaveConfiguration()
        {
            caveType = CaveType.Isometric;
            mapParameters = new MapParameters();
            floorHeight = new HeightMapProperties(DEFAULT_FLOOR_HEIGHT);
            ceilingHeight = new HeightMapProperties(DEFAULT_CEILING_HEIGHT);
            scale = DEFAULT_SCALE;
            debugMode = DEFAULT_DEBUG_MODE;
        }

        public void OnValidate()
        {
            scale = Mathf.Max(scale, MIN_SCALE);
            mapParameters.OnValidate();
            floorHeight.OnValidate();
            ceilingHeight.OnValidate();
        }

        /// <summary>
        /// Creates and returns a copy of this object. The materials are shallow copied, everything else is 
        /// deep copied. 
        /// </summary>
        public CaveConfiguration Clone()
        {
            CaveConfiguration copy = (CaveConfiguration)MemberwiseClone();
            copy.mapParameters = mapParameters.Clone();
            return copy;
        }
    }
}