using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using CaveGeneration.MapGeneration;

namespace CaveGeneration.Modules
{
    /// <summary>
    /// A module decorator that adds entrances to the underlying map generator.
    /// </summary>
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + "EntranceCarver")]
    public sealed class MapGenEntranceCarver : MapGenModule
    {
        [Tooltip("This is the module for which entrances are being carved.")]
        [SerializeField] MapGenModule mapGenerator;

        [SerializeField] int seed;

        [Tooltip("The entrances/exists themselves. Select a side of the boundary, then the X or Y value along that side.")]
        [SerializeField] BoundaryPoint[] entrances;

        /// <summary>
        /// This is the module for which entrances are being carved.
        /// </summary>
        public MapGenModule MapGenerator
        {
            get { return mapGenerator; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                mapGenerator = value;
            }
        }

        public override int Seed { set { seed = value; } }

        public IEnumerable<BoundaryPoint> GetBoundaryPoints()
        {
            return entrances;
        }

        public void SetBoundaryPoints(params BoundaryPoint[] points)
        {
            entrances = points.ToArray();
        }

        public override Map Generate()
        {
            mapGenerator.Seed = seed;
            Map map = mapGenerator.Generate();
            Boundary boundary = new Boundary(map.Length, map.Width);
            foreach (var point in entrances)
            {
                Coord coord = point.ToCoord(boundary);
                map.CarveSquare(coord);
            }
            MapBuilder.ConnectFloors(map, seed);
            return map;
        }

        void OnValidate()
        {
            foreach (var entrance in entrances)
            {
                entrance.Magnitude = Mathf.Max(0, entrance.Magnitude);
            }
        }
    } 
}