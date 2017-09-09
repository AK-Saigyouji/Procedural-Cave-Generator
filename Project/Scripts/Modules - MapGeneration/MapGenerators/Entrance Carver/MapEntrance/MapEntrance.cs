using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;

namespace AKSaigyouji.Modules.MapGeneration
{
    [Serializable]
    public struct MapEntrance
    {
        public BoundaryPoint StartPoint { get { return startPoint; } }
        public BoundaryPoint EndPoint { get { return endPoint; } }

        public MapEntrance Complement { get { return new MapEntrance(startPoint.Complement, endPoint.Complement); } }

        [SerializeField] BoundaryPoint startPoint;
        [SerializeField] BoundaryPoint endPoint;

        /// <summary>
        /// Define a map entrance between these two points. Note that start and end do not have to be on the same side.
        /// </summary>
        public MapEntrance(BoundaryPoint startPoint, BoundaryPoint endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }

        /// <summary>
        /// Define a map entrance starting at the startpoint, of the given length. e.g. passing in (Left, 15) for the
        /// start point and 3 for the length will create an entrance between (Left, 15) and (Left, 18).
        /// </summary>
        public MapEntrance(BoundaryPoint startPoint, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            this.startPoint = startPoint;
            endPoint = new BoundaryPoint(startPoint.BoundarySide, startPoint.Magnitude + length);
        }

        public MapEntrance(BoundaryPoint.Side side, int magnitude, int length) 
            : this(new BoundaryPoint(side, magnitude), length) { }

        /// <summary>
        /// Get all the coordinates corresponding to this entrance, for the given boundary.
        /// </summary>
        public IEnumerable<Coord> GetCoords(Boundary boundary)
        {
            Coord start = startPoint.ToCoord(boundary);
            Coord end = endPoint.ToCoord(boundary);
            if (startPoint.BoundarySide == endPoint.BoundarySide)
            {
                return start.GetLineTo(end);
            }
            // Things are a little (lot) more complicated if the points lie on different sides of the boundary, as we need
            // to wrap around rather than carve a single straight line. We do this by finding the corners between
            // the two points, and then carving straight lines between successive pairs of points.
            else
            {
                int xMax = boundary.xMax;
                int yMax = boundary.yMax;
                var midPoints = new List<Coord>();
                int mask = ConvertToMask(startPoint.BoundarySide) ^ ConvertToMask(endPoint.BoundarySide);
                // This switch is terrible, but it's not obvious how to avoid all the explicit case-checking 
                // without setting up a gratuitous amount of infrastructure just to avoid it. We're trying to find
                // the corners between the two points along the shortest path (clockwise vs counterclockwise). 
                // e.g. if one point is on top and the other is on the right, we know we need to go top->topright->right.
                // If points are on opposite sides, e.g. left and right, we have to go either left->topleft->topright->right
                // or left->botleft->botright->right, depending on which path is shorter. We also need to check if we're
                // starting at right or left, as the midpoints may need to be reversed.
                switch (mask)
                {
                    case 3: // top and right
                        midPoints.Add(boundary.TopRight);
                        break;
                    case 5: // top and bottom
                        bool goRight = start.x + end.x > xMax;
                        if (goRight)
                        {
                            midPoints.Add(boundary.TopRight);
                            midPoints.Add(boundary.BotRight);
                        }
                        else
                        {
                            midPoints.Add(boundary.TopLeft);
                            midPoints.Add(boundary.BotLeft);
                        }
                        if (startPoint.BoundarySide == BoundaryPoint.Side.Bottom)
                        {
                            midPoints.Reverse();
                        }
                        break;
                    case 9: // top and left
                        midPoints.Add(boundary.TopLeft);
                        break;
                    case 6: // right and bottom
                        midPoints.Add(boundary.BotRight);
                        break;
                    case 10: // right and left
                        bool goUp = start.y + end.y > yMax;
                        if (goUp)
                        {
                            midPoints.Add(boundary.TopLeft);
                            midPoints.Add(boundary.TopRight);
                        }
                        else
                        {
                            midPoints.Add(boundary.BotLeft);
                            midPoints.Add(boundary.BotRight);
                        }
                        if (startPoint.BoundarySide == BoundaryPoint.Side.Right)
                        {
                            midPoints.Reverse();
                        }
                        break;
                    case 12: // bottom and left
                        midPoints.Add(boundary.BotLeft);
                        break;
                    default:
                        throw new Exception("Internal bug: broken switch logic.");
                }
                Coord current = start;
                IEnumerable<Coord> result = Enumerable.Empty<Coord>();
                midPoints.Add(end);
                foreach (Coord coord in midPoints)                                 
                {
                    result = result.Concat(current.GetLineTo(coord));
                    current = coord;
                }
                return result;
            }
        }

        static int ConvertToMask(BoundaryPoint.Side side)
        {
            switch (side)
            {
                case BoundaryPoint.Side.Top:
                    return 1;
                case BoundaryPoint.Side.Right:
                    return 2;
                case BoundaryPoint.Side.Bottom:
                    return 4;
                case BoundaryPoint.Side.Left:
                    return 8;
                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException();
            }
        }

        public override string ToString()
        {
            return string.Format("Entrance from {0} to {1}.", startPoint, endPoint);
        }
    } 
}