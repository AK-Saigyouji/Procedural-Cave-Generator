using System;
using AKSaigyouji.HeightMaps;
using UnityEngine;

namespace AKSaigyouji.Modules.HeightMaps
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenuPath + "Rocky")]
    public sealed class HeightMapRocky : HeightMapModule
    {
        [SerializeField] LayeredNoiseParameters properties = new LayeredNoiseParameters();
        [SerializeField] int seed;

        public LayeredNoiseParameters Properties
        {
            get { return properties; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                properties = value;
            }
        }

        public override int Seed { get { return seed; } set { seed = value; } }

        public override IHeightMap GetHeightMap()
        {
            return HeightMapFactory.BuildLayeredPerlin(properties, seed);
        }

        void Reset()
        {
            properties = new LayeredNoiseParameters();
        }

        void OnValidate()
        {
            properties.OnValidate();
        }
    } 
}
