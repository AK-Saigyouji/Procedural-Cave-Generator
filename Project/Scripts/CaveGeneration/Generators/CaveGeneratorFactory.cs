using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using AKSaigyouji.MeshGeneration;

namespace AKSaigyouji.CaveGeneration
{
    public sealed class CaveGeneratorFactory
    {
        readonly MeshGenerator meshGenerator = new MeshGenerator();

        public ThreeTierCaveConfiguration GetConfigForThreeTierCaveGen()
        {
            return new ThreeTierCaveConfiguration();
        }

        public CaveGenerator BuildThreeTierCaveGen(ThreeTierCaveConfiguration config)
        {
            return new ThreeTieredCaveGenerator(meshGenerator, config);
        }

        public RockCaveConfiguration GetConfigForOutlineCaveGen()
        {
            return new RockCaveConfiguration();
        }

        public CaveGenerator BuildOutlineCaveGen(RockCaveConfiguration config)
        {
            return new OutlineCaveGenerator(meshGenerator, config);
        }
    } 
}