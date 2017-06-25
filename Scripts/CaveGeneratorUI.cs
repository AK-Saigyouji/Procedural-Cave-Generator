﻿using System.Linq;
using UnityEngine;
using UnityEditor;
using AKSaigyouji.Modules;
using AKSaigyouji.Modules.Outlines;
using AKSaigyouji.Modules.HeightMaps;
using AKSaigyouji.Modules.MapGeneration;
using System.Text;

namespace AKSaigyouji.CaveGeneration
{   
    /// <summary>
    /// Interface to the cave generator through the inspector. Can generate via code, but can only customize
    /// through inspector. Use CaveGenerator directly to customze through code.
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
            cave.transform.parent = transform;
            return cave;
        }

        /// <summary>
        /// Generate a cave whose outline consists of distinct rocks.
        /// </summary>
        public GameObject GenerateRockCave()
        {
            GameObject cave = CaveGenerator.GenerateRockCave(rockCaveConfig, randomize);
            cave.transform.parent = transform;
            return cave;
        }

        [ContextMenu("Insert Sample Modules")]
        void PopulateSamples()
        {
            Module[] modules = AssetDatabase.FindAssets("Sample t:Module")
                                            .Select(AssetDatabase.GUIDToAssetPath)
                                            .Select(AssetDatabase.LoadAssetAtPath<Module>)
                                            .ToArray();

            MapGenModule mapGenerator = modules.FirstOrDefault(m => m.name == "SampleMapGenerator") as MapGenModule;
            HeightMapModule floor = modules.FirstOrDefault(m => m.name == "SampleFloorHeightMap") as HeightMapModule;
            HeightMapModule ceiling = modules.FirstOrDefault(m => m.name == "SampleCeilingHeightMap") as HeightMapModule;
            OutlineModule outline = modules.FirstOrDefault(m => m.name == "SampleOutline") as OutlineModule;

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