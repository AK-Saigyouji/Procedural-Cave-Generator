using UnityEngine;
using CaveGeneration.MapGeneration;
using System;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + "Cellular Automata (Default)")]
    public sealed class MapGenCellAutomata : MapGenModule, IRandomizable
    {
        [SerializeField]
        [HideInInspector]
        MapParameters properties = new MapParameters();

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

        const int TUNNEL_RADIUS = 1;

        public override Map Generate()
        {
            Map map = MapBuilder.InitializeRandomMap(properties.Length, properties.Width, properties.InitialDensity, properties.Seed);
            map.TransformBoundary((x, y) => Tile.Wall);
            MapBuilder.Smooth(map);
            MapBuilder.RemoveSmallFloorRegions(map, properties.MinFloorSize);
            MapBuilder.ConnectFloors(map, properties.Seed, TUNNEL_RADIUS);
            MapBuilder.RemoveSmallWallRegions(map, properties.MinWallSize);
            map = MapBuilder.ApplyBorder(map, properties.BorderSize);
            return map;
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
