using System;
using UnityEngine;

namespace CaveGeneration
{    
    public class CaveGeneratorUI : MonoBehaviour
    {
        [SerializeField] CaveConfiguration caveConfig = new CaveConfiguration();

        [Tooltip("Select to automatically randomize the seeds of components that use seed values.")]
        [SerializeField] bool RandomizeSeeds = true;

        /// <summary>
        /// Generate a cave using the assigned properties. The game object associated with the cave will be a child
        /// of this generator.
        /// </summary>
        /// <param name="destroyPrevious">Each call to generate produces a game object that lives in the hierarchy.
        /// If set to true, this will ensure that object is destroyed, effectively being replaced by the next one.</param>
        public Cave Generate()
        {
            if (RandomizeSeeds)
            {
                caveConfig.RandomizeSeeds();
            }
            Cave cave = CaveGenerator.Generate(caveConfig);
            cave.GameObject.transform.parent = transform;
            return cave;
        }

        void OnValidate()
        {
            caveConfig.OnValidate();
        }

        void Reset()
        {
            caveConfig.Reset();
        }
    } 
}