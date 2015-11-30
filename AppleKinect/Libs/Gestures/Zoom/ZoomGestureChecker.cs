using System;
using System.Collections.Generic;
using System.Diagnostics;
using GestureDetector.DataSources;
using GestureDetector.Events;

namespace GestureDetector.Gestures.Zoom
{
    class ZoomGestureChecker: GestureChecker
    {
        protected const int ConditionTimeout = 2500;

        public override event EventHandler<GestureEventArgs> Successful;
        public override event EventHandler<FailedGestureEventArgs> Failed;

        private double _start;
        private double _last;
        private const double EPSILON = 0.01;

        public ZoomGestureChecker(Person p)
            : base(new List<Condition> {

                new ZoomCondition(p)

            }, ConditionTimeout, p)
        {
            _start = -1.0;
        }

        /// <summary>
        /// Calculating different zoom factors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void FireSucessful(object sender, GestureEventArgs e)
        {
            if(Math.Abs(_start - -1.0) < EPSILON)
            {
                _start = ((InternalZoomGestureEventArgs)e).Gauge;
                _last = ((InternalZoomGestureEventArgs)e).Gauge;
            }
            else
            {
                //Debug.WriteLine("start: " + start + " last: " + last);
                if (Successful != null)
                    Successful(this, new ZoomGestureEventArgs
                        {
                        ZoomFactorFromBegin = ((InternalZoomGestureEventArgs)e).Gauge/_start,
                        ZoomFactorFromLast = ((InternalZoomGestureEventArgs)e).Gauge/_last
                    });
                _last = ((InternalZoomGestureEventArgs)e).Gauge;
            }
        }

        protected override void FireFailed(Object sender, FailedGestureEventArgs e)
        {
            _start = -1.0;
            //Debug.WriteLine("reset zoom");
            if (Failed != null) Failed(this, e);
        }
    }
}
