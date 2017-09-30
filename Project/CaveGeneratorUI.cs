﻿/* At the moment we are handling multiple types of cave (two in this case) UIs in a single class (this one). This is
 * not too troublesome with just two types, but if multiple types were implemented, then each should get their own 
 * UI class, and this class could be written as a facade to present a single interface that hooks into the others. 
 * Otherwise the code will get too messy.*/

using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using AKSaigyouji.Modules;
using AKSaigyouji.Modules.Outlines;
using AKSaigyouji.Modules.HeightMaps;
using AKSaigyouji.Modules.MapGeneration;

namespace AKSaigyouji.CaveGeneration
{   
    /// <summary>
    /// Interface to the cave generator through the inspector.
    /// </summary>
    public sealed class CaveGeneratorUI : MonoBehaviour
    {
        public enum CaveGeneratorType
        {
            ThreeTiered,
            RockOutline
        }

        public ThreeTierCaveConfiguration ThreeTierConfig
        {
            get { return threeTierCaveConfig; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                threeTierCaveConfig = value;
            }
        }

        public RockCaveConfiguration RockConfig
        {
            get { return rockCaveConfig; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                rockCaveConfig = value;
            }
        }

        public bool Randomize { get { return randomize; } set { randomize = value; } }

        // Note: changing the name of any serialized variable may break the custom inspector (CaveGeneratorUIEditor). 

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
            var caveGenerator = new CaveGenerator();
            GameObject cave = caveGenerator.GenerateThreeTierCave(threeTierCaveConfig, randomize);
            cave.transform.parent = transform;
            return cave;
        }

        /// <summary>
        /// Generate a cave whose outline consists of distinct rocks.
        /// </summary>
        public GameObject GenerateRockCave()
        {
            var caveGenerator = new CaveGenerator();
            GameObject cave = caveGenerator.GenerateRockCave(rockCaveConfig, randomize);
            cave.transform.parent = transform;
            return cave;
        }

        // This creates an option in the UI's context menu to automatically find the sample modules in the project
        // and slot them into the UI. 
        [ContextMenu("Insert Sample Modules")]
        void PopulateSamples()
        {
            Module[] modules = AssetDatabase.FindAssets("Sample t:Module")
                                            .Select(AssetDatabase.GUIDToAssetPath)
                                            .Select(AssetDatabase.LoadAssetAtPath<Module>)
                                            .ToArray();

            Func<string, Module> getModule = (name) => modules.FirstOrDefault(module => module.name == name);

            var mapGenerator = getModule("SampleMapGenerator")     as MapGenModule;
            var floor        = getModule("SampleFloorHeightMap")   as HeightMapModule;
            var ceiling      = getModule("SampleCeilingHeightMap") as HeightMapModule;
            var outline      = getModule("SampleOutline")          as OutlineModule;

            Undo.RecordObject(this, "Insert sample modules");
            var missingModules = new StringBuilder();
            if (mapGenerator != null)
            {
                threeTierCaveConfig.MapGenerator = mapGenerator;
                rockCaveConfig.MapGenerator = mapGenerator;
            }
            else
            {
                missingModules.AppendLine("Sample map generator module not found.");
            }

            if (floor != null)
            {
                threeTierCaveConfig.FloorHeightMapModule = floor;
                rockCaveConfig.HeightMapModule = floor;
            }
            else
            {
                missingModules.AppendLine("Sample height map module for floor not found.");
            }

            if (ceiling != null)
            {
                threeTierCaveConfig.CeilingHeightMapModule = ceiling;
            }
            else
            {
                missingModules.AppendLine("Sample height map module for ceiling not found.");
            }

            if (outline != null)
            {
                rockCaveConfig.OutlineModule = outline;
            }
            else
            {
                missingModules.AppendLine("Sample outline module not found.");
            }

            if (missingModules.Length > 0)
            {
                Debug.LogError(missingModules.ToString());
            }
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