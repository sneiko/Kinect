using System;
using GestureDetector.DataSources;

namespace GestureDetector.Events
{
    /// <summary>
    /// New skeletons for Kinect arrived
    /// </summary>
    public class NewSkeletonEventArgs: EventArgs
    {
        public NewSkeletonEventArgs(SmothendSkeleton skeleton)
        {
            Skeleton = skeleton;
        }

        public SmothendSkeleton Skeleton { get; private set; }
    }
}
