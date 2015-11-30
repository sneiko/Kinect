using System;
using GestureDetector.DataSources;
using GestureDetector.Gestures;

namespace GestureDetector.Events
{
    /// <summary>
    /// Details about a gesture event.</summary>
    public abstract class GestureEventArgs: EventArgs
    {
        public Person Person { get; set; }
    }

    /// <summary>
    /// Details about a failing gesture part</summary>
    public class FailedGestureEventArgs: GestureEventArgs
    {
        public Condition Condition { get; set; }
    }
}
