using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.MapGeneration;
using AKSaigyouji.Maps;

namespace AKSaigyouji.Modules.MapGeneration
{
    [CreateAssetMenu(fileName = "Box", menuName = rootMenupath + "Box")]
    public sealed class MapGenBox : MapGenModule
    {
        public int Length
        {
            get { return length; }
            set
            {
                if (length < 0) throw new ArgumentOutOfRangeException("value");
                length = value;
            }
        }

        public int Width
        {
            get { return width; }
            set
            {
                if (width < 0) throw new ArgumentOutOfRangeException("value");
                width = value;
            }
        }

        [SerializeField] int length;
        [SerializeField] int width;

        const int BORDER_SIZE = 1;

        public override Map Generate()
        {
            Map map = new Map(length, width);
            map = MapBuilder.ApplyBorder(map, BORDER_SIZE);
            return map;
        }

        public override Coord GetMapSize()
        {
            return new Coord(length + 2 * BORDER_SIZE, width + 2 * BORDER_SIZE);
        }
    }
}