using System.Collections.Generic;
using GestureDetector.DataSources;

namespace GestureDetector.Gestures.Wave
{
    class WaveGestureChecker: GestureChecker
    {
        protected const int ConditionTimeout = 2500;

        public WaveGestureChecker(Person p)
            : base(new List<Condition> {

                new WaveRightCondition(p),
                new WaveLeftCondition(p)

            }, ConditionTimeout, p) { }
    }
}
