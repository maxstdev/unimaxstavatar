using UnityEngine;
using UMA;
using DG.Tweening;

namespace Maxst.Avatar
{
    public class AvatarPerspectiveCamera : InjectorBehaviour
    {
        [DI(DIScope.singleton)] protected AvatarCustomViewModel avatarCustomViewModel { get; }

        [SerializeField] private WardrobeSlotSO wardrobeSlotSO;
        [SerializeField] private Transform target;

        [SerializeField] private float mouseSensitivity = 0.1f;

        [SerializeField] private float distanceFromTarget = 1.9f;

        [SerializeField] private float scrollMinClamp = -1.0f;
        [SerializeField] private float scrollMaxClamp = 15f;

        private UMAData umaData;
        private Vector3 defaultCamPosition;

        public enum targetOpts { Head, Chest, Spine, Hips, LeftFoot, LeftHand, LeftLowerArm, LeftLowerLeg, LeftShoulder, LeftUpperArm, LeftUpperLeg, RightFoot, RightHand, RightLowerArm, RightLowerLeg, RightShoulder, RightUpperArm, RightUpperLeg }
        public targetOpts TargetBone;

        private string[] targetStrings = { "Head", "Chest", "Spine", "Hips", "LeftFoot", "LeftHand", "LeftLowerArm", "LeftLowerLeg", "LeftShoulder", "LeftUpperArm", "LeftUpperLeg", "RightFoot", "RightHand", "RightLowerArm", "RightLowerLeg", "RightShoulder", "RightUpperArm", "RightUpperLeg" };

        public Vector3 Offset;
        public float distance = 5.0f;

        [SerializeField] private float centerY = 0.9f;
        [SerializeField] private float paraboladistance = 1f;

        // Start is called before the first frame update
        private void Awake()
        {
            avatarCustomViewModel.ViewTypeLiveEvent.AddObserver(this, AvatarCameraMove);
        }

        void Start()
        {
            avatarCustomViewModel.AvatarViewMouseSroll.AddObserver(this, MouseScroll);
            defaultCamPosition = transform.localPosition;
        }

        private void OnDisable()
        {
            avatarCustomViewModel.AvatarViewMouseSroll.RemoveAllObserver(this);
            avatarCustomViewModel.ViewTypeLiveEvent.RemoveAllObserver(this);
        }

        private Vector3 GetTarget(Transform dstTarget = null)
        {
            Transform t = target.transform;
            if (dstTarget != null)
                t = dstTarget;

            //if (!string.IsNullOrEmpty(TargetBone))
            if (TargetBone >= 0)
            {
                if (dstTarget != null)
                {
                    umaData = dstTarget.GetComponent<UMAData>();
                }
                else
                {
                    umaData = target.GetComponent<UMAData>();
                }

                if (umaData != null && umaData.umaRecipe != null && umaData.umaRecipe.raceData != null && umaData.umaRecipe.raceData.umaTarget == RaceData.UMATarget.Humanoid && umaData.skeleton != null)
                {
                    string boneName = umaData.umaRecipe.raceData.TPose.BoneNameFromHumanName(targetStrings[(int)TargetBone]);
                    if (!string.IsNullOrEmpty(boneName))
                    {
                        var bone = umaData.skeleton.GetBoneGameObject(Animator.StringToHash(boneName));
                        if (bone != null)
                            t = bone.transform;
                    }
                }
            }

            Vector3 dest = t.position + Offset;
            return dest;
        }

        public void ZoomInFace()
        {
            distanceFromTarget = scrollMinClamp;
            //ZoomInOut();
            ParabolaMove(0.3f);
        }

        public void ZoomOutBody()
        {
            distanceFromTarget = scrollMaxClamp;
            //ZoomInOut();
            ParabolaMove(0.7f);
        }

        private void AvatarCameraMove(ViewType viewType)
        {
            var avatarCameraAnimationSO = AvatarCameraAnimationSO.Instance;
            switch (viewType)
            {
                case ViewType.Face:
                case ViewType.Hair:
                    DOPath(avatarCameraAnimationSO.FacePosition, avatarCameraAnimationSO.FaceWaypoint);
                    break;
                case ViewType.Body:
                    DOPath(avatarCameraAnimationSO.BodyPosition, avatarCameraAnimationSO.BodyWaypoint);
                    break;
                case ViewType.Animation:
                    DOPath(avatarCameraAnimationSO.AnimationPosition, avatarCameraAnimationSO.AnimationWaypoint);
                    break;
                case ViewType.Result:
                case ViewType.Init:
                    DOPath(avatarCameraAnimationSO.ResultPosition, avatarCameraAnimationSO.ResultWaypoint);
                    break;
                default: break;
            }
        }

        private void MouseScroll(Vector2 scollwheel)
        {
            distanceFromTarget = Mathf.Clamp(distanceFromTarget - scollwheel.y * mouseSensitivity, scrollMinClamp, scrollMaxClamp);
            ZoomInOut();
        }

        private void ZoomInOut()
        {
            var getTarget = GetTarget(target);
            var dis = getTarget - defaultCamPosition;
            var dir = dis.normalized;

            var fixPosition = getTarget - dir * distanceFromTarget;
            fixPosition.x = 0f;

            transform.DOLocalMove(fixPosition, 0.5f).SetEase(Ease.OutQuad);
        }

        private void ParabolaMove(float pivotvalue)
        {
            var getTarget = GetTarget(target);
            var dis = getTarget - defaultCamPosition;
            var dir = dis.normalized;

            var fixPosition = getTarget - dir * distanceFromTarget;
            fixPosition.x = 0f;

            var distance = Vector3.Distance(transform.position, fixPosition);
            Vector3 pivotposition = fixPosition;

            if (distance > paraboladistance)
            {
                pivotposition = Vector3.Lerp(transform.position, fixPosition, pivotvalue);
                pivotposition.y *= centerY;
            }

            transform.DOPath(new Vector3[] { pivotposition, fixPosition }, 0.5f, PathType.CatmullRom);
        }

        private void DOPath(Vector3 position, Vector3 waypoint)
        {
            transform.DOPath(new Vector3[] { position + waypoint, position + waypoint * 2 }, 0.5f, PathType.CatmullRom);
        }
    }
}