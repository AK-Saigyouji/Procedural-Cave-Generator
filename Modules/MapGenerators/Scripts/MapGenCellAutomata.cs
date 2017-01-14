using UnityEngine;
using CaveGeneration.MapGeneration;
using System;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + "Cellular Automata (Default)")]
    public sealed class MapGenCellAutomata : MapGenModule, IRandomizable
    {
        [SerializeField] MapParameters mapParameters = new MapParameters();
        public MapParameters Properties
        {
            get { return mapParameters; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                mapParameters = value;
            }
        }

        public override Map Generate()
        {
            return MapGenerator.GenerateMap(mapParameters);
        }

        int IRandomizable.Seed { set { mapParameters.Seed = value; } }

        void Reset()
        {
            mapParameters = new MapParameters();
        }

        void OnValidate()
        {
            mapParameters.OnValidate();
        }
    } 
}
