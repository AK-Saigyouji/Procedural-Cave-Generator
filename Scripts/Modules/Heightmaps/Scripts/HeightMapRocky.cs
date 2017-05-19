﻿using System;
using CaveGeneration.MeshGeneration;
using CaveGeneration.HeightMaps;
using UnityEngine;

namespace CaveGeneration.Modules
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

        public override int Seed { set { seed = value; } }

        public override IHeightMap GetHeightMap()
        {
            return properties.ToHeightMap(seed);
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
