using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.MapGeneration;
using AKSaigyouji.Maps;

namespace AKSaigyouji.Modules.MapGeneration
{
    /// <summary>
    /// A module decorator that adds entrances to the underlying map generator.
    /// </summary>
    [CreateAssetMenu(fileName = fileName, menuName = rootMenupath + "EntranceCarver")]
    public sealed class MapGenEntranceCarver : MapGenModule
    {
        [Tooltip("This is the module for which entrances are being carved.")]
        [SerializeField]
        MapGenModule carvedModule;

        [Tooltip("The radius of any tunnels needed to connect carved entrances to the rest of the map, if applicable.")]
        [SerializeField] int tunnelRadius;

        [SerializeField] int seed;

        [Tooltip("The entrances/exits themselves. Select a side of the boundary, then the X or Y value along that side.")]
        [SerializeField]
        MapEntrance[] entrances;

        /// <summary>
        /// This is the module for which entrances are being carved.
        /// </summary>
        public MapGenModule MapGenerator
        {
            get { return carvedModule; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                carvedModule = value;
            }
        }

        /// <summary>
        /// Must be at least 1.
        /// </summary>
        public int TunnelRadius
        {
            get { return tunnelRadius; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");
                tunnelRadius = value;
            }
        }

        public override int Seed { get { return seed; } set { seed = value; } }

        public override IEnumerable<MapEntrance> GetOpenings()
        {
            return entrances.Concat(carvedModule.GetOpenings());
        }

        public void SetOpenings(params MapEntrance[] points)
        {
            entrances = points.ToArray();
        }

        public override Coord GetMapSize()
        {
            return carvedModule.GetMapSize();
        }

        public override IEnumerable<Coord> GetBoundary()
        {
            return carvedModule.GetBoundary();
        }

        public override Map Generate()
        {
            if (carvedModule == null)
                throw new InvalidOperationException("Must assign a map generator to carve entrances for.");

            carvedModule.Seed = seed;
            Map map = carvedModule.Generate();
            Boundary boundary = new Boundary(map.Length, map.Width);
            int entranceLength = 0;
            foreach (MapEntrance entrance in entrances)
            {
                foreach (Coord coord in entrance.GetCoords(boundary))
                {
                    entranceLength++;
                    map[coord] = Tile.Floor;
                }
            }
            MapBuilder.ConnectFloors(map, seed: seed, tunnelRadius: tunnelRadius);
            return map;
        }

        void OnValidate()
        {
            tunnelRadius = Mathf.Max(1, tunnelRadius);

            if (carvedModule != null)
            {
                Coord extrema = carvedModule.GetMapSize() - Coord.One;
                for (int i = 0; i < entrances.Length; i++)
                {
                    MapEntrance entrance = entrances[i];

                    BoundaryPoint start = entrance.StartPoint;
                    BoundaryPoint end = entrance.EndPoint;

                    var startSide = start.BoundarySide;
                    int startMagnitude = ClampMagnitude(start, extrema);
                    BoundaryPoint newStart = new BoundaryPoint(startSide, startMagnitude);

                    var endSide = end.BoundarySide;
                    int endMagnitude = ClampMagnitude(end, extrema);
                    BoundaryPoint newEnd = new BoundaryPoint(endSide, endMagnitude);

                    entrances[i] = new MapEntrance(newStart, newEnd);
                }
            }
        }

        static int ClampMagnitude(BoundaryPoint point, Coord extrema)
        {
            int max = point.IsHorizontal() ? extrema.x : extrema.y;
            return Mathf.Clamp(point.Magnitude, 0, max);
        }
    } 
}