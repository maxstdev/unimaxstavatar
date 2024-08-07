using DG.Tweening;
using UnityEngine;

namespace Maxst.Avatar
{
    public class LookAtFaceCam : MonoBehaviour
    {
        public Transform characterHead;
        public Vector3 offset = new Vector3(0, 0, 1);
        public float distanceControl = 1.0f;

        private GameObject target;

        private void Start()
        {
            target = new GameObject("target");
            target.transform.parent = characterHead;

            var newpos = new Vector3(0, 0.2f, 0);
            target.transform.localPosition = newpos;
            target.transform.localRotation = Quaternion.identity;
        }

        void LateUpdate()
        {
            if (characterHead != null)
            {
                Vector3 targetPosition = target.transform.position + (-target.transform.right * offset.z * distanceControl);
                targetPosition += target.transform.right * offset.x;
                targetPosition += target.transform.up * offset.y;

                transform.position = targetPosition;

                transform.LookAt(target.transform);
            }
        }
    }
}
