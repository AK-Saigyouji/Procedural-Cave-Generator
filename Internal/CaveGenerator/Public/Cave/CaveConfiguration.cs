using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;
using CaveGeneration.Modules;
using System;
using UnityEngine;

namespace CaveGeneration
{
    [Serializable]
    /// <summary>
    /// Complete set of serializable information necessary to build a cave. 
    /// </summary>
    public sealed class CaveConfiguration
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

        public HeightMapModule FloorHeightMap
        {
            get { return floorHeightMap; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                floorHeightMap = value;
            }
        }

        public HeightMapModule CeilingHeightMap
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

        public CaveType CaveType
        {
            get { return caveType; }
            set { caveType = value; }
        }

        #endregion

        [SerializeField] MapGenModule mapGenerator;
        [SerializeField] HeightMapModule floorHeightMap;
        [SerializeField] HeightMapModule ceilingHeightMap;
        [SerializeField] Material floorMaterial;
        [SerializeField] Material wallMaterial;
        [SerializeField] Material ceilingMaterial;
        [SerializeField] int scale;
        [SerializeField] CaveType caveType;

        const int MIN_SCALE = 1;
        const int DEFAULT_SCALE = 1;

        public CaveConfiguration()
        {
            scale = DEFAULT_SCALE;
        }

        /// <summary>
        /// Copies the configuration. Materials receive a shallow copy, everything else receives a deep copy.
        /// </summary>
        public CaveConfiguration Clone()
        {
            var newConfig = (CaveConfiguration)MemberwiseClone();
            newConfig.mapGenerator = ScriptableObject.Instantiate(mapGenerator);
            newConfig.floorHeightMap = ScriptableObject.Instantiate(floorHeightMap);
            newConfig.ceilingHeightMap = ScriptableObject.Instantiate(ceilingHeightMap);
            return newConfig;
        }

        /// <summary>
        /// This will randomize the seed of components that use a seed value.
        /// </summary>
        public void RandomizeSeeds()
        {
            int randomSeed = Guid.NewGuid().GetHashCode();
            TrySetSeed(mapGenerator, randomSeed);
            TrySetSeed(floorHeightMap, randomSeed);
            TrySetSeed(ceilingHeightMap, randomSeed);
        }

        void TrySetSeed(ScriptableObject module, int seed)
        {
            IRandomizable randomizable = module as IRandomizable;
            if (randomizable != null)
            {
                randomizable.Seed = seed;
            }
        }

        internal void OnValidate()
        {
            scale = Mathf.Max(MIN_SCALE, scale);
        }

        internal void Reset()
        {
            scale = MIN_SCALE;
        }
    } 
}