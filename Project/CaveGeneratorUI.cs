/* At the moment we are handling multiple types of cave (two in this case) UIs in a single class (this one). This is
 * not too troublesome with just two types, but if multiple types were implemented, then each should get their own 
 * UI class, and this class could be written as a facade to present a single interface that hooks into the others. 
 * Otherwise the code will get too messy.*/

using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using AKSaigyouji.Modules;
using AKSaigyouji.Modules.CaveWalls;
using AKSaigyouji.Modules.Outlines;
using AKSaigyouji.Modules.HeightMaps;
using AKSaigyouji.Modules.MapGeneration;

namespace AKSaigyouji.CaveGeneration
{   
    /// <summary>
    /// Interface to the cave generator through the inspector.
    /// </summary>
    public class CaveGeneratorUI : MonoBehaviour
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

        [SerializeField] CaveGeneratorType type;

        [SerializeField] ThreeTierCaveConfiguration threeTierCaveConfig;
        [SerializeField] RockCaveConfiguration rockCaveConfig;

        [Tooltip("Select to automatically randomize the seeds of components that use seed values.")]
        [SerializeField] bool randomize = true;

        CaveGeneratorFactory caveGeneratorFactory;

        void Awake()
        {
            caveGeneratorFactory = new CaveGeneratorFactory();
        }

        public GameObject Generate()
        {
            switch (type)
            {
                case CaveGeneratorType.ThreeTiered:
                    return GenerateThreeTier();
                case CaveGeneratorType.RockOutline:
                    return GenerateRockCave();
                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException();
            }
        }

        GameObject GenerateThreeTier()
        {
            if (randomize)
                threeTierCaveConfig.SetSeed(GetRandomSeed());

            CaveGenerator caveGenerator = caveGeneratorFactory.BuildThreeTierCaveGen(threeTierCaveConfig);
            return GenerateCave(caveGenerator);
        }

        GameObject GenerateRockCave()
        {
            if (randomize)
                rockCaveConfig.SetSeed(GetRandomSeed());
            
            CaveGenerator caveGenerator = caveGeneratorFactory.BuildOutlineCaveGen(rockCaveConfig);
            return GenerateCave(caveGenerator);
        }

        GameObject GenerateCave(CaveGenerator generator)
        {
            GameObject cave = generator.Generate();
            cave.transform.parent = transform;
            return cave;
        }

        int GetRandomSeed()
        {
            return Guid.NewGuid().GetHashCode();
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
            var wall         = getModule("SampleWallModule")       as CaveWallModule;

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

            if (wall != null)
            {
                threeTierCaveConfig.WallModule = wall;
            }
            else
            {
                missingModules.AppendLine("Sample wall module not found.");
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