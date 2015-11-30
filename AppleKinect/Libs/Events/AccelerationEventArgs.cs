using System;

namespace GestureDetector.Events
{
    /// <summary>
    /// Gets fired when the devices sensor has other readings. Ignores skeletons.
    /// </summary>
    public class AccelerationEventArgs: EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount">Acceleration distance</param>
        public AccelerationEventArgs(double amount)
        {
            Amount = amount;
        }

        /// <summary>
        /// Sum of all the differences
        /// </summary>
        public double Amount { get; private set; }
    }
}
