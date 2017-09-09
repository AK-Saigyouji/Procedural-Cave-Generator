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
        [SerializeField] int tunnelRadius = 1;

        [SerializeField] int seed;

        [Tooltip("The entrances/exits themselves. Select a side of the boundary, then the X or Y value along that side.")]
        [SerializeField]
        MapEntrance[] entrances;

        bool[,] cachedBools;

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
            if (carvedModule == null)
                throw new InvalidOperationException("Module not assigned.");

            return entrances.Concat(carvedModule.GetOpenings());
        }

        public void SetOpenings(params MapEntrance[] openings)
        {
            entrances = openings.ToArray();
        }

        /// <param name="openings">Can be empty not but null.</param>
        /// <param name="tunnelRadius">Must be at least 1.</param>
        public static MapGenEntranceCarver Construct(MapGenModule mapGenerator, IEnumerable<MapEntrance> openings, int tunnelRadius = 1)
        {
            if (mapGenerator == null)
                throw new ArgumentNullException("mapGenerator");

            if (openings == null)
                throw new ArgumentNullException("openings");

            if (tunnelRadius < 1)
                throw new ArgumentOutOfRangeException("tunnelRadius", "Tunnel radius must be at least 1.");

            var carver = CreateInstance<MapGenEntranceCarver>();
            carver.entrances = openings.ToArray();
            carver.MapGenerator = mapGenerator;
            carver.tunnelRadius = tunnelRadius;
            return carver;
        }

        public override Coord GetMapSize()
        {
            if (carvedModule == null)
                throw new InvalidOperationException("Module not assigned.");

            return carvedModule.GetMapSize();
        }

        public override IEnumerable<Coord> GetBoundary()
        {
            if (carvedModule == null)
                throw new InvalidOperationException("Module not assigned.");

            return carvedModule.GetBoundary();
        }

        public override Map Generate()
        {
            if (carvedModule == null)
                throw new InvalidOperationException("Must assign a map generator to carve entrances for.");

            carvedModule.Seed = seed;
            Map map = carvedModule.Generate();
            Boundary boundary = new Boundary(map.Length, map.Width);
            ITunneler tunneler = MapTunnelers.GetRandomDirectedTunneler(boundary, seed, MapTunnelers.Variance.Low);
            foreach (MapEntrance entrance in entrances)
            {
                int numCoords = entrance.GetCoords(boundary).Count();
                int halfwayIndex = Mathf.Min(numCoords - 1, numCoords / 2);
                Coord midPoint = entrance.GetCoords(boundary).Skip(halfwayIndex).First();
                ConnectEntrance(map, midPoint, tunneler);
                foreach (Coord coord in entrance.GetCoords(boundary))
                {
                    map[coord] = Tile.Floor;
                }
            }
            return map;
        }

        void OnValidate()
        {
            tunnelRadius = Mathf.Max(1, tunnelRadius);

            if (carvedModule != null && entrances != null)
            {
                Coord extrema;
                try
                {
                    extrema = carvedModule.GetMapSize() - Coord.one;
                }
                catch (InvalidOperationException)
                {
                    // this catches the case where a module is slotted into the entrance carver in the inspector,
                    // but the module is not fully configured and throws an exception when accessing its map size.
                    return;
                }
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

        void ConnectEntrance(Map map, Coord coord, ITunneler tunneler)
        {
            Coord target = FindNearbyFloor(map, coord);
            var bdry = new Boundary(map.Length, map.Width);
            foreach (Coord pathTile in tunneler.GetPath(coord, target))
            {
                for (int x = pathTile.x - tunnelRadius; x < pathTile.x + tunnelRadius; x++)
                {
                    Coord tile = new Coord(x, pathTile.y);
                    if (bdry.IsInBounds(tile)) map[tile] = Tile.Floor;
                }
                for (int y = pathTile.y - tunnelRadius; y < pathTile.y + tunnelRadius; y++)
                {
                    Coord tile = new Coord(pathTile.x, y);
                    if (bdry.IsInBounds(tile)) map[tile] = Tile.Floor;
                }
            }
        }

        Coord FindNearbyFloor(Map map, Coord start)
        {
            var visited = GetBools(map.Length, map.Width);
            visited[start.x, start.y] = true;
            var bdry = new Boundary(map.Length, map.Width);
            var q = new Queue<Coord>();
            var neighbours = new Coord[] { new Coord(1, 0), new Coord(0, 1), new Coord(-1, 0), new Coord(0, -1) };
            foreach (Coord neighbour in neighbours.Select(n => n + start).Where(bdry.IsInBounds))
            {
                q.Enqueue(neighbour);
                visited[neighbour.x, neighbour.y] = true;
            }
            while (q.Count > 0)
            {
                Coord curr = q.Dequeue();
                if (map.IsFloor(curr))
                {
                    return curr;
                }
                foreach (Coord n in neighbours)
                {
                    Coord neighbour = n + curr;
                    if (bdry.IsInBounds(neighbour) && !visited[neighbour.x, neighbour.y])
                    {
                        q.Enqueue(neighbour);
                        visited[neighbour.x, neighbour.y] = true;
                    }
                }
            }
            throw new ArgumentException("Map has no floors aside from the start coord.");
        }

        // retrieves cached bools if available and of the right size, otherwise allocates new
        bool[,] GetBools(int length, int width)
        {
            if (cachedBools != null && cachedBools.GetLength(0) == length && cachedBools.GetLength(1) == width)
            {
                Array.Clear(cachedBools, 0, cachedBools.Length);
            }
            else
            {
                cachedBools = new bool[length, width];
            }
            return cachedBools;
        }

        static int ClampMagnitude(BoundaryPoint point, Coord extrema)
        {
            int max = point.IsHorizontal() ? extrema.x : extrema.y;
            return Mathf.Clamp(point.Magnitude, 0, max);
        }
    } 
}