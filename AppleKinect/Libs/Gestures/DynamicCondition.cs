using System;
using GestureDetector.DataSources;
using GestureDetector.Events;

namespace GestureDetector.Gestures
{
    /// <summary>
    /// Continuous gestures succeed after a certain time or on a certain condition. 
    /// Before that, they just trigger that they occured.
    /// 
    /// Gesture Sequence:
    /// 
    /// -------|----------|----------|-----|------|--------|----->
    ///        T          T          T     S      T        T
    /// 
    /// T = fireTriggered
    /// S = fireSuccess
    /// 
    /// An example is the zoom gesture. We always need feedback for the zooming factor, 
    /// but we cant succeed. Therefore we trigger an occurence. The zoom gesture succeeds
    /// when zooming ended.
    /// </summary>
    public abstract class DynamicCondition: Condition
    {
        public DynamicCondition(Person p): base(p)
        { 
        }

        #region Events
        
        /// <summary>
        /// Called when a gesture just occured but neighter succeeded nor failed.</summary>
        public event EventHandler<GestureEventArgs> Triggered;

        /// <summary>
        /// Trigger this to signal the occurence of a gesture part.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void FireTriggered(object sender, GestureEventArgs e)
        {
            if (Triggered != null)
            {
                Triggered(sender, e);
            }
        }

        #endregion
    }
}
