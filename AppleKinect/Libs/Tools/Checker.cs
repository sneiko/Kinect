using System;
using System.Collections.Generic;
using GestureDetector.DataSources;
using Microsoft.Kinect;

namespace GestureDetector.Tools
{
    /// <summary>
    /// Library for skeleton based vector math like joint velocity
    /// </summary>
    public class Checker
    {
        private Person person;

        /// <summary>
        /// Instantiates a Checker with a Person to check on.</summary>
        /// <param name="p">
        /// The Person with a set of Kinect skeletons to check on.</param>
        public Checker(Person p)
        {
            person = p;
        }

        /// <summary>
        /// Get a joints absolute velocity. If theres not enough skeleton information,
        /// precision is decreased automatically.</summary>
        /// <param name="type">
        /// The JointType to get velocity from.</param>
        /// <returns>
        /// Returns the absolute velocity in meters</returns>
        public double GetAbsoluteVelocity(JointType type)
        {
            if (!HasSkeleton(1))
            {
                return 0;
            }
            if (!HasSkeleton(3))
            {
                return SimpleAbsoluteVelocity(type, 0, 1);
            }
            return SkeletonMath.Median(SimpleAbsoluteVelocity(type, 0, 1), SimpleAbsoluteVelocity(type, 1, 2), SimpleAbsoluteVelocity(type, 2,3 ));
        }

        /// <summary>
        /// Simply calculates the velocity of a joint. Takes two versions.</summary>
        /// <param name="type">
        /// The JointType to get velocity from.</param>
        /// <param name="firstTime">
        /// First index of the persons cached skeletons</param>
        /// <param name="secondTime">
        /// Second index of the persons cached skeletons</param>
        /// <returns>
        /// Returns the absolute velocity in meters</returns>
        private double SimpleAbsoluteVelocity(JointType type, int firstTime, int secondTime)
        {
            if (!HasSkeleton(firstTime) || !HasSkeleton(secondTime))
            {
                throw new ArgumentException("No Skeleton at this Index");
            }
            return SkeletonMath.DistanceBetweenPoints(person.GetLastSkeleton(firstTime).GetPosition(type), person.GetLastSkeleton(secondTime).GetPosition(type)) * 1000.0 / person.MillisBetweenFrames(1, 0);
        }

        /// <summary>
        /// Calculates the relative velocity of a joint referencing to a second one.</summary>
        /// <param name="steady">
        /// The referenctial JointType.</param>
        /// <param name="moving">
        /// The moving JointType of interest</param>
        /// <returns>
        /// Returns the relative velocity in meters</returns>
        public double GetRelativeVelocity(JointType steady, JointType moving)
        {
            if (!HasSkeleton(1))
            {
                return 0;
            }
            SkeletonPoint d0 = SubstractedPointsAt(steady, moving, 0);
            SkeletonPoint d1 = SubstractedPointsAt(steady, moving, 1);
            if (!HasSkeleton(3))
            {
                return SkeletonMath.DistanceBetweenPoints(d0, d1) * 1000.0 / person.MillisBetweenFrames(1, 0);
            }
            SkeletonPoint d2 = SubstractedPointsAt(steady, moving, 2);
            SkeletonPoint d3 = SubstractedPointsAt(steady, moving, 3);
            return SkeletonMath.Median(
                SkeletonMath.DistanceBetweenPoints(d0, d1) * 1000.0 / person.MillisBetweenFrames(1, 0),
                SkeletonMath.DistanceBetweenPoints(d1, d2) * 1000.0 / person.MillisBetweenFrames(2, 1),
                SkeletonMath.DistanceBetweenPoints(d2, d3) * 1000.0 / person.MillisBetweenFrames(3, 2)
            );
        }

        /// <summary>
        /// Checks if a person has a skeleton for a given time.</summary>
        /// <param name="time">
        /// Index of the persons skeleton cache.</param>
        /// <returns>
        /// Returns true if there is a skeleton for the given index, false otherwise.</returns>
        private bool HasSkeleton(int time)
        {
            return person.GetLastSkeleton(time) != null;
        }

        private SkeletonPoint SubstractedPointsAt(JointType steady, JointType moving, int time)
        {
            if (!HasSkeleton(time))
            {
                throw new ArgumentException("No Skeleton at this Index");
            }
            return SkeletonMath.SubstractPoints(person.GetLastSkeleton(time).GetPosition(moving), person.GetLastSkeleton(time).GetPosition(steady));
        }

        /// <summary>
        /// Median of the distance between two points
        /// Median over 3
        /// </summary>
        /// <param name="t1">Joint 1</param>
        /// <param name="t2">Joint 2</param>
        /// <returns>Distance in Meters</returns>
        public double GetDistanceMedian(JointType t1, JointType t2)
        {
            if (!HasSkeleton(2))
            {
                return GetDistance(t1, t2);
            }
            return SkeletonMath.Median(GetDistance(t1,t2), GetDistance(t1,t2,1), GetDistance(t1,t2,2));
        }

        /// <summary>
        /// The last distance between the points. 
        /// </summary>
        /// <param name="t1">Joint 1</param>
        /// <param name="t2">Joint 2</param>
        /// <returns>Distance in Meters</returns>
        public double GetDistance(JointType t1, JointType t2)
        {
            return GetDistance(t1, t2, 0);
        }

        private double GetDistance(JointType t1, JointType t2, int time)
        {
            if (!HasSkeleton(time))
            {
                throw new ArgumentException("No Skeleton at this Index");
            }
            return SkeletonMath.DistanceBetweenPoints(person.GetLastSkeleton(time).GetPosition(t1), person.GetLastSkeleton(time).GetPosition(t2));
        }

        /// <summary>
        /// The actual movement directions
        /// High Tolerance
        /// </summary>
        /// <param name="type">Joint</param>
        /// <returns>Enumerable of the directions</returns>
        public IEnumerable<Direction> GetAbsoluteMovement(JointType type)
        {
            if (!HasSkeleton(1))
            {
                return new List<Direction> { Direction.None };
            }
            return SkeletonMath.DirectionTo(person.GetLastSkeleton(1).GetPosition(type), person.CurrentSkeleton.GetPosition(type));
        }

        /// <summary>
        /// The actual direction of movement to a relative point
        /// </summary>
        /// <param name="steady">The reference joint</param>
        /// <param name="moving">The joint for the direction</param>
        /// <returns>Enumerable of the directions</returns>
        public IEnumerable<Direction> GetRelativeMovement(JointType steady, JointType moving)
        {
            if (!HasSkeleton(1))
            {
                return new List<Direction> { Direction.None };
            }
            return SkeletonMath.DirectionTo(SkeletonMath.SubstractPoints(person.GetLastSkeleton(1).GetPosition(moving), person.GetLastSkeleton(1).GetPosition(steady)),
                SkeletonMath.SubstractPoints(person.CurrentSkeleton.GetPosition(moving), person.CurrentSkeleton.GetPosition(steady)));
        }

        /// <summary>
        /// The static position of a joint in relation to another
        /// </summary>
        /// <param name="from">source of the direction</param>
        /// <param name="to">target of the direction</param>
        /// <returns>Enumerable of the directions</returns>
        public IEnumerable<Direction> GetRelativePosition(JointType from, JointType to)
        {
            return SkeletonMath.DirectionTo(person.CurrentSkeleton.GetPosition(from), person.CurrentSkeleton.GetPosition(to));
        }

        /// <summary>
        /// Direction of a joint over a span of frames
        /// Low tolerance, but movement has to be constant
        /// </summary>
        /// <param name="type">the joint</param>
        /// <param name="duration">number of frames</param>
        /// <returns>Enumerable of the directions</returns>
        public IEnumerable<Direction> GetSteadyAbsoluteMovement(JointType type, int duration)
        {
            List<SkeletonPoint> from = new List<SkeletonPoint>(), to = new List<SkeletonPoint>();
            if (duration < 1)
            {
                throw new ArgumentException("Duration must be at least 1");
            }
            for (int i = 0; i < duration && HasSkeleton(i); i++)
			{
                to.Add(person.GetLastSkeleton(i).GetPosition(type));
			    from.Add(person.GetLastSkeleton(i+1).GetPosition(type));
			}
            return SkeletonMath.SteadyDirectionTo(from, to);
        }

        /// <summary>
        /// Relative movement over a timespawn
        /// Low tolerance, but movement has to be constant
        /// </summary>
        /// <param name="steady">The reference joint</param>
        /// <param name="moving">The joint to get the direction from</param>
        /// <param name="duration">Timespawn in frames</param>
        /// <returns>Enumerable of the directions</returns>
        public IEnumerable<Direction> GetSteadyRelativeMovement(JointType steady, JointType moving, int duration)
        {
            List<SkeletonPoint> from = new List<SkeletonPoint>(), to = new List<SkeletonPoint>();
            if (duration < 1)
            {
                throw new ArgumentException("Duration must be at least 1");
            }
            for (int i = 0; i < duration && HasSkeleton(i); i++)
            {
                to.Add(SkeletonMath.SubstractPoints(person.GetLastSkeleton(i).GetPosition(moving), person.GetLastSkeleton(i).GetPosition(steady)));
                from.Add(SkeletonMath.SubstractPoints(person.GetLastSkeleton(i+1).GetPosition(moving), person.GetLastSkeleton(i+1).GetPosition(steady)));
            }
            return SkeletonMath.SteadyDirectionTo(from, to);
        }

        /// <summary>
        /// The relative position over a timespawn
        /// Low tolerance, but the position has to be constant
        /// </summary>
        /// <param name="from">Source of the direction</param>
        /// <param name="to">target of the direction</param>
        /// <param name="duration">Timespawn in frames</param>
        /// <returns>Enumerable of the directions</returns>
        public IEnumerable<Direction> GetSteadyPosition(JointType from, JointType to, int duration)
        {
            List<SkeletonPoint> origin = new List<SkeletonPoint>(), target = new List<SkeletonPoint>();
            if (duration < 0)
            {
                throw new ArgumentException("Duration must be at least 1");
            }
            for (int i = 0; i <= duration && HasSkeleton(i); i++)
            {
                target.Add(person.GetLastSkeleton(i).GetPosition(to));
                origin.Add(person.GetLastSkeleton(i).GetPosition(from));
            }
            return SkeletonMath.SteadyDirectionTo(origin, target);
        }
    }
}
