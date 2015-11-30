using System.Collections.Generic;
using System.Linq;
using GestureDetector.DataSources;
using GestureDetector.Events;
using GestureDetector.Tools;
using Microsoft.Kinect;

namespace GestureDetector.Gestures.Swipe
{
    /// <summary>
    /// Swipe Condition
    /// Checks if  left or right hand moves fast to the left or the right.
    /// </summary>
    class SwipeCondition: DynamicCondition
    {
        private readonly JointType _hand;
        private Direction _direction;

        private readonly Checker _checker;
        private const int LowerBoundForSuccess = 2;
        private const double LowerBoundForVelocity = 2.0;
        private int _index;

        public SwipeCondition(Person p, JointType leftOrRightHand)
            : base(p)
        {
            _index = 0;
            _hand = leftOrRightHand;
            _checker = new Checker(p);
        }

        protected override void Check(object sender, NewSkeletonEventArgs e)
        {
            List<Direction> handToHipOrientation = _checker.GetRelativePosition(JointType.HipCenter, _hand).ToList();
            List<Direction> handToShoulderOrientation = _checker.GetRelativePosition(JointType.ShoulderCenter, _hand).ToList();
            List<Direction> handMovement = _checker.GetAbsoluteMovement(_hand).ToList();
            double handVelocity = _checker.GetRelativeVelocity(JointType.HipCenter,_hand);
            if (handVelocity < LowerBoundForVelocity)
            {
                Reset();
            }
                // hand is in front of the body and between hip and shoulders
            else if (handToHipOrientation.Contains(Direction.Forward)
               // && handToHipOrientation.Contains(Direction.Upward)
                && handToShoulderOrientation.Contains(Direction.Downward))
            {
                // movement did not start yet, initializing
                if (_direction == Direction.None)
                {
                    // left or right movement is prefered
                    if (handMovement.Contains(Direction.Left))
                    {
                        _direction = Direction.Left;
                    }
                    else if (handMovement.Contains(Direction.Right))
                    {
                        _direction = Direction.Right;
                    }
                    else
                    {
                        // take other direction
                        //direction = handMovement.FirstOrDefault();
                    }
                }
                else if (!handMovement.Contains(_direction))
                {
                    // direction changed
                    Reset();
                }
                else
                {
                    if (_index >= LowerBoundForSuccess)
                    {
                        _index = 0;
                        FireSucceeded(this, new SwipeGestureEventArgs
                            {
                            Direction = _direction
                        });
                        _direction = Direction.None;
                    }
                    else
                    {
                        // step successful, waiting for next
                        _index++;
                    }
                }
            }
        }
        // restart detecting
        private void Reset()
        {
            _index = 0;
            _direction = Direction.None;
            FireFailed(this, new FailedGestureEventArgs
                {
                Condition = this
            });
        }
    }
}
