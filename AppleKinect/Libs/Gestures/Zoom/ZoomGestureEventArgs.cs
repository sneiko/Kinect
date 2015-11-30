using GestureDetector.Events;

namespace GestureDetector.Gestures.Zoom
{
    /// <summary>
    /// Contains the calculated Zoom-factors
    /// </summary>
    public class ZoomGestureEventArgs : GestureEventArgs
    {
        /// <summary>
        /// The factor calculated from the beginning of the gesture
        /// </summary>
        public double ZoomFactorFromBegin { get; set; }
        /// <summary>
        /// The factor since the last frame
        /// </summary>
        public double ZoomFactorFromLast { get; set; }
    }
}
