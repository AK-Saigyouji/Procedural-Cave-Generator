﻿using CaveGeneration.MeshGeneration;
using CaveGeneration.HeightMaps;
using UnityEngine;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenuPath + "Constant")]
    public sealed class HeightMapConstant : HeightMapModule
    {
        [SerializeField] float height;
        public float Height
        {
            get { return height; }
            set { height = value; }
        }

        public override IHeightMap GetHeightMap()
        {
            return HeightMapFactory.Build(height);
        }
    } 
}
