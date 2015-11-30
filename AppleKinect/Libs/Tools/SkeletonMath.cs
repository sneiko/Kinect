using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;

namespace GestureDetector.Tools
{
    /// <summary>
    /// Abstract directions for joint movement</summary>
    public enum Direction
    {
        /// <summary>
        /// A direction along the z-axis
        /// </summary>
        Forward, 
        /// <summary>
        /// A direction along the z-axis
        /// </summary>
        Backward,
        /// <summary>
        /// A direction along the y-axis
        /// </summary>
        Upward, 
        /// <summary>
        /// A direction along the y-axis
        /// </summary>
        Downward, 
        /// <summary>
        /// A direction along the x-axis
        /// </summary>
        Left, 
        /// <summary>
        /// A direction along the x-axis
        /// </summary>
        Right, 
        /// <summary>
        /// No direction
        /// </summary>
        None
    }

    /// <summary>
    /// Library for independent vector aritmetics</summary>
    class SkeletonMath
    {
        public const double Tolerance = 0.06;
        public const double MedianTolerance = 0.01;
        public const double MedianCorrectNeeded = 0.66666666;

        /// <summary>
        /// Get distance of two skeleton points in meters.</summary>
        public static double DistanceBetweenPoints(SkeletonPoint p1, SkeletonPoint p2)
        {
            double dx = Math.Abs(p2.X - p1.X);
            double dy = Math.Abs(p2.Y - p1.Y);
            double dz = Math.Abs(p2.Z - p1.Z);
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static SkeletonPoint SubstractPoints(SkeletonPoint op1, SkeletonPoint op2)
        {
            SkeletonPoint res = new SkeletonPoint();
            res.X = op1.X - op2.X;
            res.Y = op1.Y - op2.Y;
            res.Z = op1.Z - op2.Z;
            return res;
        }

        public static SkeletonPoint AddPoints(SkeletonPoint op1, SkeletonPoint op2)
        {
            SkeletonPoint res = new SkeletonPoint();
            res.X = op1.X + op2.X;
            res.Y = op1.Y + op2.Y;
            res.Z = op1.Z + op2.Z;
            return res;
        }

        /// <summary>
        /// the median over three values
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="d3"></param>
        /// <returns></returns>
        public static double Median(double d1, double d2, double d3)
        {
            // more performance than copying and sorting
            if ((d1 > d2 && d1 < d3) || (d1 < d2 && d1 > d3))
            {
                return d1;
            }
            if ((d2 > d1 && d2 < d3) || (d2 < d1 && d2 > d3))
            {
                return d2;
            }
            return d3;
        }

        /// <summary>
        /// The median over a unspecified number of values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Median(IEnumerable<double> values)
        {
            List<double> d = values.ToList();
            d.Sort();
            return d[d.Count / 2];
        }

        /// <summary>
        /// The median of the direction between points
        /// </summary>
        /// <param name="from">Source joints</param>
        /// <param name="to">Target joints</param>
        /// <returns></returns>
        public static IEnumerable<Direction> SteadyDirectionTo(IEnumerable<SkeletonPoint> from, IEnumerable<SkeletonPoint> to)
        {
            List<List<Direction>> directions = new List<List<Direction>>();
            var origin = from.ToList();
            var target = to.ToList();
            if (from.Count() != to.Count())
            {
                throw new ArgumentException("Length not identical");
            }
            for (int i = 0; i < from.Count(); i++)
            {
                directions.Add(new List<Direction>());
                double dx = target[i].X - origin[i].X;
                double dy = target[i].Y - origin[i].Y;
                double dz = target[i].Z - origin[i].Z;
                if (dx > MedianTolerance)
                {
                    directions[i].Add(Direction.Right);
                }
                else if (dx < -MedianTolerance)
                {
                    directions[i].Add(Direction.Left);
                }
                if (dy > MedianTolerance)
                {
                    directions[i].Add(Direction.Upward);
                }
                else if (dy < -MedianTolerance)
                {
                    directions[i].Add(Direction.Downward);
                }
                if (dz > MedianTolerance)
                {
                    directions[i].Add(Direction.Backward);
                }
                else if (dz < -MedianTolerance)
                {
                    directions[i].Add(Direction.Forward);
                }
                
            }
            List<Direction> res = new List<Direction>();
            foreach (Direction item in Enum.GetValues(typeof(Direction)))
            {
                // found enough times in lists
                if (directions.Count(x => x.Contains(item)) > origin.Count * MedianCorrectNeeded)
                {
                    res.Add(item);
                }
            }
            if (res.Count == 0)
            {
                res.Add(Direction.None);
            }
            return res;
        }

        /// <summary>
        /// Get an abstract direction type between two skeleton points</summary>
        /// <param name="from">
        /// Source Point</param>
        /// <param name="to">
        /// Target Point</param>
        /// <returns>
        /// Returns a list of three directions (for each axis)</returns>
        public static IEnumerable<Direction> DirectionTo(SkeletonPoint from, SkeletonPoint to)
        {
            List<Direction> res = new List<Direction>();
            double dx = to.X - from.X;
            double dy = to.Y - from.Y;
            double dz = to.Z - from.Z;
            if (dx > Tolerance)
            {
                res.Add(Direction.Right);
            }
            else if (dx < -Tolerance)
            {
                res.Add(Direction.Left);
            }
            if (dy > Tolerance)
            {
                res.Add(Direction.Upward);
            }
            else if (dy < -Tolerance)
            {
                res.Add(Direction.Downward);
            }
            if (dz > Tolerance)
            {
                res.Add(Direction.Backward);
            }
            else if (dz < -Tolerance)
            {
                res.Add(Direction.Forward);
            }
            if (res.Count == 0)
            {
                res.Add(Direction.None);
            }
            return res;
        }
    }
}
