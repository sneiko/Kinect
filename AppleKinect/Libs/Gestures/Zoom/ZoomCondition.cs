using System.Collections.Generic;
using System.Linq;
using GestureDetector.DataSources;
using GestureDetector.Events;
using GestureDetector.Tools;
using Microsoft.Kinect;

namespace GestureDetector.Gestures.Zoom
{
    class ZoomCondition : DynamicCondition
    {
        private const double UpperBoundForVelocity = 1.5;
        private const int LowerBoundToBegin = 30;
        private int _index;
        // TODO set Distance, positive = zoomOut, neg = zoomIn
        //private int zoomDistance = 0;
        private Checker checker;
        List<Direction> _rightHandToHipOrientation, _leftHandToHipOrientation, _rightHandToHeadOrientation,
            _leftHandToHeadOrientation, _leftHandToRightHandDirection;
        double _rightHandVelocity, _leftHandVelocity;

        public ZoomCondition(Person p)
            : base(p)
        {
            _index = 0;
            checker = new Checker(p);
        }

        protected override void Check(object sender, NewSkeletonEventArgs e)
        {
            _rightHandToHipOrientation = checker.GetRelativePosition(JointType.HipLeft, JointType.HandRight).ToList();
            _leftHandToHipOrientation = checker.GetRelativePosition(JointType.HipRight, JointType.HandLeft).ToList();
            _rightHandToHeadOrientation = checker.GetRelativePosition(JointType.Head, JointType.HandRight).ToList();
            _leftHandToHeadOrientation = checker.GetRelativePosition(JointType.Head, JointType.HandLeft).ToList();
            _leftHandToRightHandDirection = checker.GetRelativePosition(JointType.HandRight, JointType.HandLeft).ToList();
            _rightHandVelocity = checker.GetRelativeVelocity(JointType.HipCenter, JointType.HandRight);
            _leftHandVelocity = checker.GetRelativeVelocity(JointType.HipCenter, JointType.HandLeft);

            // Both Hands have to be in front of the body, between head and hip, and on their corresponding side
            // but not together or crossed
            if (_rightHandToHipOrientation.Contains(Direction.Forward)
                && _leftHandToHipOrientation.Contains(Direction.Forward)
                && _rightHandToHipOrientation.Contains(Direction.Upward)
                && _leftHandToHipOrientation.Contains(Direction.Upward)
                && _rightHandToHipOrientation.Contains(Direction.Right)
                && _leftHandToHipOrientation.Contains(Direction.Left)
                && _rightHandToHeadOrientation.Contains(Direction.Downward)
                && _leftHandToHeadOrientation.Contains(Direction.Downward)
                && _leftHandToRightHandDirection.Contains(Direction.Left)
                && (_rightHandVelocity <= UpperBoundForVelocity)
                && (_leftHandVelocity <= UpperBoundForVelocity))
            {

                if (_index >= LowerBoundToBegin)
                {
                    FireSucceeded(this, new InternalZoomGestureEventArgs
                        {
                        Gauge = checker.GetDistanceMedian(JointType.HandRight, JointType.HandLeft)
                    });
                    //Console.WriteLine("Success! Velocity: right " + rightHandVelocity + ", left " + leftHandVelocity);
                }
                else
                {
                    _index++;
                    FireTriggered(this, new InternalZoomGestureEventArgs
                    {
                        Gauge = checker.GetDistanceMedian(JointType.HandRight, JointType.HandLeft)
                    });
                }
            }
            else
            {
                _index -= 11;
                if (_index < 0)
                {
                    _index = 0;
                    FireFailed(this, new FailedGestureEventArgs
                        {
                            Condition = this
                        });
                }
            }
        }
    }
}
