using System;
using UnityEngine;
using AKSaigyouji.Modules.HeightMaps;
using AKSaigyouji.Modules.MapGeneration;
using AKSaigyouji.Modules.Outlines;
using System.Text;

namespace AKSaigyouji.CaveGeneration
{
    [Serializable]
    public sealed class RockCaveConfiguration
    {
        [SerializeField] MapGenModule mapGenerator;
        [SerializeField] HeightMapModule floorHeightMap;
        [SerializeField] OutlineModule outlineModule;
        [SerializeField] int scale;
        [SerializeField] Material material;

        const int MIN_SCALE = 1;
        const int DEFAULT_SCALE = 1;

        public MapGenModule MapGenerator
        {
            get { return mapGenerator; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                mapGenerator = value;
            }
        }

        public HeightMapModule HeightMapModule
        {
            get { return floorHeightMap; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                floorHeightMap = value;
            }
        }

        public OutlineModule OutlineModule
        {
            get { return outlineModule; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                outlineModule = value;
            }
        }

        /// <summary>
        /// Scales up generated floor. Does not scale up the instantiated rocks. Min value of 1: values below this
        /// threshold will be clamped to 1.
        /// </summary>
        public int Scale
        {
            get { return scale; }
            set { scale = Mathf.Max(1, value); }
        }

        /// <summary>
        /// The material that will be applied to the floors.
        /// </summary>
        public Material Material { get { return material; } set { material = value; } }

        public RockCaveConfiguration()
        {
            scale = DEFAULT_SCALE;
        }

        public void SetSeed(int seed)
        {
            mapGenerator.Seed = seed;
            floorHeightMap.Seed = seed;
            outlineModule.Seed = seed;
        }

        /// <summary>
        /// Check if all the properties are in valid state, i.e. ready to generate a cave. If so, returns empty string.
        /// Otherwise, returns a string indicating why the state is invalid.
        /// </summary>
        public string Validate()
        {
            var sb = new StringBuilder();

            if (mapGenerator == null)
                sb.AppendLine("No map generator assigned.");

            if (floorHeightMap == null)
                sb.AppendLine("No height map assigned.");

            return sb.ToString();
        }

        internal void OnValidate()
        {
            scale = Mathf.Max(MIN_SCALE, scale);
        }
    } 
}