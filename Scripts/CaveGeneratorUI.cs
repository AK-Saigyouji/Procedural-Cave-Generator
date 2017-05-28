/* This is the UI to the cave generator in the inspector. It has the restriction of only being customizable 
 through the inspector, despite exposing its generate methods through code. The reason for this is that
 through custom editors, it's possible to expose only the properties corresponding to the selected generator type.
 There is nothing analogous in code: it would be necessary to expose all properties for all generator types, leading
 to an error prone configuration setup. Instead, customization through code is done by working directly with
 CaveGenerator, not CaveGeneratorUI.*/

using System;
using UnityEngine;

namespace CaveGeneration
{   
    /// <summary>
    /// Interface to the cave generator through the inspector. Can generate via code, but can only customize
    /// through inspector. 
    /// </summary>
    public sealed class CaveGeneratorUI : MonoBehaviour
    {
        public enum CaveGeneratorType
        {
            ThreeTiered,
            RockOutline
        }

        // Note: changing the name of any properties may break the custom inspector (CaveGeneratorUIEditor). 

        [SerializeField] CaveGeneratorType type;

        [SerializeField] ThreeTierCaveConfiguration threeTierCaveConfig;
        [SerializeField] RockCaveConfiguration rockCaveConfig;

        [Tooltip("Select to automatically randomize the seeds of components that use seed values.")]
        [SerializeField] bool randomize = true;

        /// <summary>
        /// Generate a three tier cave: a distinct mesh will be produced for the floor, walls and ceiling. 
        /// Furthermore, large caves will be broken up into sectors which each have their own floor/wall/ceiling.
        /// </summary>
        public GameObject GenerateThreeTier()
        {
            GameObject cave = CaveGenerator.GenerateThreeTierCave(threeTierCaveConfig, randomize);
            cave.SetParent(transform);
            return cave;
        }

        /// <summary>
        /// Generate a cave whose outline consists of distinct rocks.
        /// </summary>
        public GameObject GenerateRockCave()
        {
            GameObject cave = CaveGenerator.GenerateRockCave(rockCaveConfig, randomize);
            cave.SetParent(transform);
            return cave;
        }

        void OnValidate()
        {
            threeTierCaveConfig.OnValidate();
            rockCaveConfig.OnValidate();
        }

        void Reset()
        {
            threeTierCaveConfig = new ThreeTierCaveConfiguration();
            rockCaveConfig = new RockCaveConfiguration();
        }
    } 
}