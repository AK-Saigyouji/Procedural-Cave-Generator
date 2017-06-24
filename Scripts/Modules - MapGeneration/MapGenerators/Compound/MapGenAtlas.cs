﻿/* This is a generator type that stitches together the maps from multiple generators into a single map. 
 An individual map (referred to in this script as a chart) carries an offset to locate it in the larger map.*/

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;

namespace AKSaigyouji.Modules.MapGeneration
{
    public sealed class MapGenAtlas : MapGenModule
    {
        [SerializeField] MapGenModule[] modules;
        [SerializeField] Vector2[] offsets;
        [SerializeField] int seed;

        public override int Seed { get { return seed; } set { seed = value; } }

        public override Map Generate()
        {
            int[] range = Enumerable.Range(0, modules.Length).ToArray();
            var random = new System.Random(seed); // This is used to deterministically generate random seeds
            Map[] charts = modules.Select(mod => mod.Generate(random.Next())).ToArray();
            int totalLength = (int)range.Max(i => charts[i].Length + offsets[i].x);
            int totalWidth = (int)range.Max(i => charts[i].Width + offsets[i].y);
            Map map = new Map(totalLength, totalWidth);
            map.Fill(Tile.Wall);
            for (int i = 0; i < charts.Length; i++)
            {
                CopyChart(map, charts[i], offsets[i]);
            }
            return map;
        }

        public override Coord GetMapSize()
        {
            var limits = Enumerable.Range(0, modules.Length).Select(i => modules[i].GetMapSize() + offsets[i]);
            int totalLength = (int)limits.Max(coord => coord.x);
            int totalWidth = (int)limits.Max(coord => coord.y);
            return new Coord(totalLength, totalWidth);
        }

        public override IEnumerable<Coord> GetBoundary()
        {
            var boundary = Enumerable.Empty<Coord>();
            for (int i = 0; i < modules.Length; i++)
            {
                Coord offset = (Coord)offsets[i];
                MapGenModule mod = modules[i];
                boundary = boundary.Concat(mod.GetBoundary().Select(coord => coord + offset));
            }
            return boundary;
        }

        public static MapGenAtlas Build(IEnumerable<MapGenModule> modules, IEnumerable<Vector2> offsets)
        {
            var mapGen = CreateInstance<MapGenAtlas>();
            mapGen.modules = modules.ToArray();
            mapGen.offsets = AnchorOffsets(offsets);
            return mapGen;
        }

        static void CopyChart(Map atlas, Map chart, Vector2 offset)
        {
            int xOffset = (int)offset.x;
            int yOffset = (int)offset.y;
            chart.ForEach((x, y) =>
            {
                if (chart.IsFloor(x, y))
                {
                    atlas[x + xOffset, y + yOffset] = Tile.Floor;
                }
            });
        }

        // Shifts the offsets uniformly so that their bounding box's bottom left corner is (0,0).
        static Vector2[] AnchorOffsets(IEnumerable<Vector2> rawOffsets)
        {
            float xMin = rawOffsets.Min(coord => coord.x);
            float yMin = rawOffsets.Min(coord => coord.y);
            Vector2[] anchoredOffsets = rawOffsets.Select(coord => new Vector2(coord.x - xMin, coord.y - yMin)).ToArray();
            return anchoredOffsets;
        }
    }
}