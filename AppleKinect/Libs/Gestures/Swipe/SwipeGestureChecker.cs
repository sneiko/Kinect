using System.Collections.Generic;
using GestureDetector.DataSources;
using Microsoft.Kinect;

namespace GestureDetector.Gestures.Swipe
{
    class SwipeGestureChecker: GestureChecker
    {
        protected const int ConditionTimeout = 1500;

        public SwipeGestureChecker(Person p)
            : base(new List<Condition> {

                new SwipeCondition(p, JointType.HandRight)

            }, ConditionTimeout, p) { }
    }
}
