using UnityEngine;
using CaveGeneration.MapGeneration;
using CaveGeneration.MapGeneration.GraphAlgorithms;
using System;
using System.Collections.Generic;

namespace CaveGeneration.Modules
{
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + "Cellular Automata (Default)")]
    public sealed class MapGenCellAutomata : MapGenModule
    {
        [SerializeField] MapParameters properties = new MapParameters();
        [SerializeField] int seed;

        public MapParameters Properties
        {
            get { return properties; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                properties = value;
            }
        }

        public override int Seed { get { return seed; } set { seed = value; } }

        public override Map Generate()
        {
            Map map = MapBuilder.InitializeRandomMap(properties.Length, properties.Width, properties.InitialDensity, seed);
            map.TransformBoundary((x, y) => Tile.Wall);
            MapBuilder.Smooth(map);
            MapBuilder.RemoveSmallFloorRegions(map, properties.MinFloorSize);
            MapBuilder.ConnectFloors(map, seed, Mathf.Max(1, properties.MinPassageRadius));
            MapBuilder.WidenTunnels(map, properties.MinPassageRadius);
            MapBuilder.RemoveSmallWallRegions(map, properties.MinWallSize);
            map = MapBuilder.ApplyBorder(map, properties.BorderSize);
            return map;
        }

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
