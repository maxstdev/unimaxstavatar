using Maxst;
using UnityEngine;

namespace Maxst.Avatar
{
    [CreateAssetMenu(fileName = "AvatarCameraAnimationSO", menuName = "ScriptableObjects/AvatarCameraAnimationSO", order = 1000)]
    public class AvatarCameraAnimationSO : ScriptableSingleton<AvatarCameraAnimationSO>
    {
        public Vector3 FacePosition;
        public Vector3 BodyPosition;
        public Vector3 AnimationPosition;
        public Vector3 ResultPosition;
        public Vector3 FaceWaypoint;
        public Vector3 BodyWaypoint;
        public Vector3 AnimationWaypoint;
        public Vector3 ResultWaypoint;
    }
}