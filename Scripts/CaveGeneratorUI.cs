using System;
using UnityEngine;

namespace CaveGeneration
{    
    public sealed class CaveGeneratorUI : MonoBehaviour
    {
        [SerializeField] CaveConfiguration caveConfig = new CaveConfiguration();

        [Tooltip("Select to automatically randomize the seeds of components that use seed values.")]
        [SerializeField] bool RandomizeSeeds = true;

        public CaveConfiguration Configuration
        {
            get { return caveConfig; }
            set
            {
                if (caveConfig == null)
                    throw new ArgumentNullException("value");

                caveConfig = value;
            }
        }

        /// <summary>
        /// Generate a cave using the assigned properties. The game object associated with the cave will be a child
        /// of this generator.
        /// </summary>
        public Cave Generate()
        {
            Cave cave = CaveGenerator.Generate(caveConfig, RandomizeSeeds);
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