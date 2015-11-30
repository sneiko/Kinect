using Microsoft.Kinect;

namespace GestureDetector.DataSources
{
    class StaticSmothendSkeleton:SmothendSkeleton
    {
        public StaticSmothendSkeleton(Skeleton s, long timestamp)
            : base(s, timestamp)
        {
        }
    }
}
