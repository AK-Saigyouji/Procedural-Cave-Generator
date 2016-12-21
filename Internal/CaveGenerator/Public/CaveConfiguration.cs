/* This serves as a container for the configurable properties of the cave generator, and handles the related
 book-keeping regarding validation, reset logic, etc.*/

using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;
using UnityEngine;

namespace CaveGeneration
{
    [System.Serializable]
    public sealed class CaveConfiguration
    {
        IHeightMap floorHeightMap;
        IHeightMap ceilingHeightMap;

        [SerializeField] CaveType caveType;
        [SerializeField] MapParameters mapParameters;

        [SerializeField] Material ceilingMaterial;
        [SerializeField] Material wallMaterial;
        [SerializeField] Material floorMaterial;

        [SerializeField] HeightMapProperties floorHeight;
        [SerializeField] HeightMapProperties ceilingHeight;

        [SerializeField, Tooltip(Tooltips.CAVE_GEN_SCALE)] int scale;
        [SerializeField, Tooltip(Tooltips.CAVE_GEN_DEBUG_MODE)] bool debugMode;

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

        public IHeightMap FloorHeightMap
        {
            get { return floorHeightMap ?? floorHeight.ToHeightMap(mapParameters.Seed); }
            set { floorHeightMap = value; }
        }

        public IHeightMap CeilingHeightMap
        {
            get { return ceilingHeightMap ?? ceilingHeight.ToHeightMap(mapParameters.Seed); }
            set { ceilingHeightMap = value; }
        }

        const int DEFAULT_SCALE = 1;
        const int MIN_SCALE = 1;
        const bool DEFAULT_DEBUG_MODE = false;
        const int DEFAULT_FLOOR_HEIGHT = 0;
        const int DEFAULT_CEILING_HEIGHT = 3;

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
        /// Creates and return a copy of this object. Performs a deep copy with the exception of the materials,
        /// which will not be duplicated.
        /// </summary>
        public CaveConfiguration Clone()
        {
            CaveConfiguration copy = (CaveConfiguration)MemberwiseClone();
            copy.mapParameters = mapParameters.Clone();
            return copy;
        }
    }
}