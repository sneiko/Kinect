using GestureDetector.Events;

namespace GestureDetector.Gestures.Zoom
{
    /// <summary>
    /// EventArgs to pass the hand distance to the GestureChecker
    /// </summary>
    internal class InternalZoomGestureEventArgs: GestureEventArgs
    {
        /// <summary>
        /// Actual distance between hands
        /// </summary>
        public double Gauge { get; set; }
    }
}
