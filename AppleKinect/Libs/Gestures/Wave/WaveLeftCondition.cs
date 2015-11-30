using System.Collections.Generic;
using System.Linq;
using GestureDetector.DataSources;
using GestureDetector.Events;
using GestureDetector.Tools;
using Microsoft.Kinect;

namespace GestureDetector.Gestures.Wave
{
    /// <summary>
    /// Conditions for a left wave</summary>
    public class WaveLeftCondition: DynamicCondition
    {
        private int _index;
        private Checker checker;
        private List<Direction> _handToHeadDirections;

        public WaveLeftCondition(Person p)
            : base(p)
        {
            _index = 0;
            checker = new Checker(p);
        }

        protected override void Check(object sender, NewSkeletonEventArgs e)
        {
            //rightHandDirections = checker.GetAbsoluteMovement(JointType.HandRight);
            _handToHeadDirections = checker.GetRelativePosition(JointType.ShoulderCenter, JointType.HandRight).ToList();
            double handspeed = checker.GetAbsoluteVelocity(JointType.HandRight);
            //Debug.WriteLine(handspeed);
            // min required speed
            if (handspeed < 2)
            {
                _index = 0;
            }
            // hand must be left
            if (_index == 0 && _handToHeadDirections.Contains(Direction.Right))
            {
                _index = 1;
            }
                // hand is on top
            else if (_index == 1 && _handToHeadDirections.Contains(Direction.Upward))
            {
                _index = 2;
            }
                //hand is right
            else if (_index == 2 && _handToHeadDirections.Contains(Direction.Right))
            {
                FireSucceeded(this, null);
                //Debug.WriteLine("triggered");
                _index = 0;
                //if (index >= LOWER_BOUND_FOR_SUCCESS)
                //{
                //    fireSucceeded(this, null);
                //}
                
            }
        }
    }
}

