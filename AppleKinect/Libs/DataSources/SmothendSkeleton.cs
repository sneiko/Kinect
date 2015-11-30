using System.Collections.Generic;
using Microsoft.Kinect;

namespace GestureDetector.DataSources
{
    /// <summary>
    /// The joint data is stored here.
    /// Filtering is also possible here
    /// </summary>
    public class SmothendSkeleton
    {
        // The joint's data
        private Dictionary<JointType, Joint> joints;
        /// <summary>
        /// Time of the Skeleton
        /// </summary>
        public long Timestamp { get; private set; }

        public SmothendSkeleton(Skeleton s, long timestamp)
        {
            Timestamp = timestamp;
            Positon = s.Position;
            TrackingId = s.TrackingId;
            TrackingState = s.TrackingState;
            if (TrackingState == SkeletonTrackingState.Tracked)
            {
                joints = new Dictionary<JointType, Joint>();
                foreach (Joint j in s.Joints)
                {
                    joints.Add(j.JointType, j);
                }
            }
        }

        public SkeletonPoint GetPosition(JointType jt)
        {
            if (TrackingState == SkeletonTrackingState.Tracked)
            {
                return joints[jt].Position;
            }
            return new SkeletonPoint();
        }

        public JointTrackingState GetState(JointType jt)
        {
            if (TrackingState == SkeletonTrackingState.Tracked)
            {
                return joints[jt].TrackingState;
            }
            return JointTrackingState.NotTracked;
        }

        internal SkeletonPoint Positon { get; private set; }

        internal int TrackingId { get; private set; }

        internal SkeletonTrackingState TrackingState { get; private set; }
    }
}
