using UnityEngine;
using CaveGeneration.MapGeneration;
using System;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + "Cellular Automata (Default)")]
    public sealed class MapGenCellAutomata : MapGenModule, IRandomizable
    {
        [SerializeField] MapParameters properties = new MapParameters();
        public MapParameters Properties
        {
            get { return properties; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                properties = value;
            }
        }

        const int BASE_TUNNEL_RADIUS = 1;

        public override Map Generate()
        {
            return MapBuilder
                .InitializeRandomMap(properties.Length, properties.Width, properties.InitialDensity, properties.Seed)
                .Smooth()
                .RemoveSmallFloorRegions(properties.MinFloorSize)
                .ExpandRegions(properties.FloorExpansion)
                .ConnectFloors(BASE_TUNNEL_RADIUS + properties.FloorExpansion)
                .SmoothOnlyWalls()
                .RemoveSmallWallRegions(properties.MinWallSize)
                .ApplyBorder(properties.BorderSize);
        }

        int IRandomizable.Seed { set { properties.Seed = value; } }

        void Reset()
        {
            properties = new MapParameters();
        }

        void OnValidate()
        {
            properties.OnValidate();
        }
    } 
}
