using Microsoft.Kinect;

namespace GestureDetector.DataSources
{
    class MovingSmothendSkeleton: SmothendSkeleton
    {
        public MovingSmothendSkeleton(Skeleton s, long timestamp)
            : base(s, timestamp)
        {
        }
    }
}
