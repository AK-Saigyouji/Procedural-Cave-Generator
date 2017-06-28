using UnityEngine;
using AKSaigyouji.MapGeneration;
using AKSaigyouji.Maps;
using System;
using System.Collections.Generic;

namespace AKSaigyouji.Modules.MapGeneration
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

        public override Coord GetMapSize()
        {
            int length = properties.Length + 2 * properties.BorderSize;
            int width = properties.Width + 2 * properties.BorderSize;
            return new Coord(length, width);
        }

        public override Map Generate()
        {
            Map map = MapBuilder.InitializeRandomMap(properties.Length, properties.Width, properties.InitialDensity, seed);
            map.TransformBoundary((x, y) => Tile.Wall);
            MapBuilder.Smooth(map);
            MapBuilder.RemoveSmallFloorRegions(map, properties.MinFloorSize);
            MapBuilder.ConnectFloors(map, seed);
            if (properties.ExpandTunnels)
            {
                MapBuilder.WidenTunnels(map);
            }
            MapBuilder.RemoveSmallWallRegions(map, properties.MinWallSize);
            map = MapBuilder.ApplyBorder(map, properties.BorderSize);
            return map;
        }

        public override IEnumerable<Coord> GetBoundary()
        {
            int length = properties.Length + 2 * properties.BorderSize;
            int width = properties.Width + 2 * properties.BorderSize;
            var boundary = new Boundary(length, width);
            return boundary.GetAllCoords();
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
