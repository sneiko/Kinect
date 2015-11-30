using GestureDetector.Events;
using GestureDetector.Tools;

namespace GestureDetector.Gestures.Swipe
{
    /// <summary>
    /// Person swiped
    /// </summary>
    public class SwipeGestureEventArgs: GestureEventArgs
    {
        /// <summary>
        /// Direction of the swipe
        /// </summary>
        public Direction Direction { get; set; }
    }
}
