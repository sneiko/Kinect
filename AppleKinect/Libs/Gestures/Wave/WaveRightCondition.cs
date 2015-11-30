using System.Collections.Generic;
using System.Linq;
using GestureDetector.DataSources;
using GestureDetector.Events;
using GestureDetector.Tools;
using Microsoft.Kinect;

namespace GestureDetector.Gestures.Wave
{
    /// <summary>
    /// The condition when waving left
    /// </summary>
    public class WaveRightCondition: DynamicCondition
    {
        private int _index;
        private Checker checker;
        List<Direction> _handToHeadDirections;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">The assosiated person</param>
        public WaveRightCondition(Person p)
            : base(p)
        {
            _index = 0;
            checker = new Checker(p);
        }

        protected override void Check(object sender, NewSkeletonEventArgs e)
        {
            checker.GetAbsoluteMovement(JointType.HandRight);
            _handToHeadDirections = checker.GetRelativePosition(JointType.ShoulderCenter, JointType.HandRight).ToList();
            // Prüfe ob Handbewegung nach links abläuft und ob sich die Hand über dem Kopf befindet
            double handspeed = checker.GetAbsoluteVelocity(JointType.HandRight);
            //Debug.WriteLine(handspeed);
            // min required speed
            if (handspeed < 2)
            {
                _index = 0;
            }
            // hand must be right
            if (_index == 0 && _handToHeadDirections.Contains(Direction.Right))
            {
                _index = 1;
            }
            // hand is on top
            else if (_index == 1 && _handToHeadDirections.Contains(Direction.Upward))
            {
                _index = 2;
            }
            //hand is left
            else if (_index == 2 && _handToHeadDirections.Contains(Direction.Left))
            {
                FireSucceeded(this, null);
                //Debug.WriteLine("triggered" + e.Skeleton.Timestamp);
                _index = 0;
                //if (index >= LOWER_BOUND_FOR_SUCCESS)
                //{
                //    fireSucceeded(this, null);
                //}

            }
        }
    }
}
