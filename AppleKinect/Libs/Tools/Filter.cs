using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace GestureDetector.Tools
{
    /// <summary>
    /// Modes for filtering
    /// </summary>
    public enum FilteringModes
    {
        /// <summary>
        /// Raw Data, not recommanded
        /// </summary>
        None,
        /// <summary>
        /// Some smoothing with little latency.
        /// Only filters out small jitters.
        /// Good for gesture recognition in games.
        /// </summary>
        Low,
        /// <summary>
        /// The default setting.
        /// Smoothed with some latency.
        /// Filters out medium jitters.
        /// Good for a menu system that needs to be smooth but
        /// doesn't need the reduced latency as much as gesture recognition does.
        /// </summary>
        Medium,
        /// <summary>
        ///  Very smooth, but with a lot of latency.
        /// Filters out large jitters.
        /// Good for situations where smooth data is absolutely required
        /// and latency is not an issue.
        /// </summary>
        Smooth,
        /// <summary>
        /// A user-defined filtering
        /// </summary>
        Custom
    }

    /// <summary>
    /// The Filter Class holds the smooting parameters for the SkeletonStream.
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// The actual smothing parameter
        /// </summary>
        public TransformSmoothParameters SmoothingParam { get; private set; }

        /// <summary>
        /// The used filter mode
        /// </summary>
        public FilteringModes Mode { get; private set; }

        /// <summary>
        /// Creating a default filter 
        /// </summary>
        public Filter()
        {
            SmoothingParam = new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            };
            Mode = FilteringModes.Low;
        }

        /// <summary>
        /// A filter of a custom set
        /// </summary>
        /// <param name="mode">The Mode to create</param>
        public Filter(FilteringModes mode)
        {
            if (mode == FilteringModes.Custom)
            {
                throw new InvalidEnumArgumentException("Can not create a custom filter without arguments");
            }
            Mode = mode;
            switch (mode)
            {
                case FilteringModes.None:
                    SmoothingParam = new TransformSmoothParameters
                        {
                            Smoothing = 0.0f,
                            Correction = 0.0f,
                            Prediction = 0.0f,
                            JitterRadius = 0.0f,
                            MaxDeviationRadius = 0.0f
                        };
                    break;
                case FilteringModes.Low:
                    SmoothingParam = new TransformSmoothParameters
                        {
                            Smoothing = 0.5f,
                            Correction = 0.5f,
                            Prediction = 0.5f,
                            JitterRadius = 0.05f,
                            MaxDeviationRadius = 0.04f
                        };
                    break;
                case FilteringModes.Medium:
                    SmoothingParam = new TransformSmoothParameters
                        {
                            Smoothing = 0.5f,
                            Correction = 0.1f,
                            Prediction = 0.5f,
                            JitterRadius = 0.1f,
                            MaxDeviationRadius = 0.1f
                        };
                    break;
                case FilteringModes.Smooth:
                    SmoothingParam = new TransformSmoothParameters
                        {
                            Smoothing = 0.7f,
                            Correction = 0.3f,
                            Prediction = 1.0f,
                            JitterRadius = 1.0f,
                            MaxDeviationRadius = 1.0f
                        };
                    break;
            }
        }

        /// <summary>
        /// Creating a Custom Filter
        /// </summary>
        /// <param name="correction">
        /// - Correction parameter. Lower values are slower to correct towards the raw data and appear smoother, while higher values will correct toward the raw data more quickly.
        /// - Values must be in the range 0 through 1.0.</param>
        /// <param name="jitter">
        /// - The radius in meters for jitter reduction.
        /// - Any jitter beyond this radius is clamped to the radius.</param>
        /// <param name="prediction">
        /// - The number of frames to predict into the future.
        /// - Values must be greater than or equal to zero.
        /// - Values greater than 0.5 will likely lead to overshooting when moving quickly. This effect can be damped by using small values of fMaxDeviationRadius.</param>
        /// <param name="smoothing">
        /// - Smoothing parameter. Increasing the smoothing parameter value leads to more highly-smoothed skeleton position values being returned.
        /// - It is the nature of smoothing that, as the smoothing value is increased, responsiveness to the raw data decreases.
        /// - Thus, increased smoothing leads to increased latency in the returned skeleton values.
        /// - Values must be in the range 0 through 1.0. Passing 0 causes the raw data to be returned.</param>
        /// <param name="deviationRadius">
        /// - The maximum radius in meters that filtered positions are allowed to deviate from raw data.
        /// - Filtered values that would be more than this radius from the raw data are clamped at this distance, in the direction of the filtered value.</param>
        public Filter(float correction, float jitter, float prediction, float smoothing, float deviationRadius)
        {
            Mode = FilteringModes.Custom;
            SmoothingParam = new TransformSmoothParameters()
                {
                    Smoothing = smoothing,
                    Correction = correction,
                    Prediction = prediction,
                    JitterRadius = jitter,
                    MaxDeviationRadius = deviationRadius
                };
        }
    }
}
