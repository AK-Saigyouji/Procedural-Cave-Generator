using System;
using CaveGeneration.MeshGeneration;
using CaveGeneration.HeightMaps;
using UnityEngine;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenuPath + "Rocky")]
    public sealed class HeightMapRocky : HeightMapModule, IRandomizable
    {
        [SerializeField] LayeredNoiseParameters properties = new LayeredNoiseParameters();

        public LayeredNoiseParameters Properties
        {
            get { return properties; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                properties = value;
            }
        }

        public override IHeightMap GetHeightMap()
        {
            return properties.ToHeightMap();
        }

        int IRandomizable.Seed { set { properties.Seed = value; } }

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
