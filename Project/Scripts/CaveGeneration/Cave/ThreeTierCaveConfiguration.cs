using AKSaigyouji.Modules.MapGeneration;
using AKSaigyouji.Modules.HeightMaps;
using System;
using System.Text;
using UnityEngine;

namespace AKSaigyouji.CaveGeneration
{
    [Serializable]
    /// <summary>
    /// Complete set of information necessary to build a cave. 
    /// </summary>
    public sealed class ThreeTierCaveConfiguration
    {
        #region properties

        public MapGenModule MapGenerator
        {
            get { return mapGenerator; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                mapGenerator = value;
            }
        }

        public HeightMapModule FloorHeightMapModule
        {
            get { return floorHeightMap; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                floorHeightMap = value;
            }
        }

        public HeightMapModule CeilingHeightMapModule
        {
            get { return ceilingHeightMap; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                ceilingHeightMap = value;
            }
        }

        public Material FloorMaterial
        {
            get { return floorMaterial; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                floorMaterial = value;
            }
        }

        public Material WallMaterial
        {
            get { return wallMaterial; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                wallMaterial = value;
            }
        }

        public Material CeilingMaterial
        {
            get { return ceilingMaterial; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                ceilingMaterial = value;
            }
        }

        public int Scale
        {
            get { return scale; }
            set
            {
                if (scale < MIN_SCALE) throw new ArgumentException("Scale must be at least 1.", "value");
                scale = value;
            }
        }

        public ThreeTierCaveType CaveType
        {
            get { return caveType; }
            set { caveType = value; }
        }

        #endregion
        // Note: changing the names of these modules will break the CaveGeneratorUI custom editor
        [SerializeField] MapGenModule mapGenerator;
        [SerializeField] HeightMapModule floorHeightMap;
        [SerializeField] HeightMapModule ceilingHeightMap;
        [SerializeField] Material floorMaterial;
        [SerializeField] Material wallMaterial;
        [SerializeField] Material ceilingMaterial;
        [SerializeField] int scale;
        [SerializeField] ThreeTierCaveType caveType;

        const int MIN_SCALE = 1;
        const int DEFAULT_SCALE = 1;

        public ThreeTierCaveConfiguration()
        {
            scale = DEFAULT_SCALE;
        }

        /// <summary>
        /// Copies the configuration. Materials receive a shallow copy, everything else receives a deep copy.
        /// </summary>
        public ThreeTierCaveConfiguration Clone()
        {
            var newConfig = (ThreeTierCaveConfiguration)MemberwiseClone();
            newConfig.mapGenerator = ScriptableObject.Instantiate(mapGenerator);
            newConfig.floorHeightMap = ScriptableObject.Instantiate(floorHeightMap);
            newConfig.ceilingHeightMap = ScriptableObject.Instantiate(ceilingHeightMap);
            return newConfig;
        }

        public void SetSeed(int seed)
        {
            mapGenerator.Seed = seed;
            floorHeightMap.Seed = seed;
            ceilingHeightMap.Seed = seed;
        }

        /// <summary>
        /// All the properties are in valid state, i.e. ready to be used to generate a cave.
        /// </summary>
        public string Validate()
        {
            var sb = new StringBuilder();

            if (mapGenerator == null)
                sb.AppendLine("No map generator assigned.");

            if (floorHeightMap == null)
                sb.AppendLine("No height map assigned for the floor.");

            if (ceilingHeightMap == null)
                sb.AppendLine("No height map assigned for the ceiling.");

            return sb.ToString();
        }

        internal void OnValidate()
        {
            scale = Mathf.Max(MIN_SCALE, scale);
        }
    } 
}