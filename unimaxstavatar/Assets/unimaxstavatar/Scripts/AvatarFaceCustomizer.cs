using System.Collections;
using System.Collections.Generic;
using UMA.CharacterSystem;
using UMA.Examples;
using UnityEngine;

namespace Maxst.Avatar
{
    public class AvatarFaceCustomizer : MonoBehaviour
    {
        private AvatarCustomView avatarcustomview;

        public DynamicCharacterAvatar avatar;
        public UMAMouseOrbitImproved orbitor;

        public DNAPanel faceEditor;
        public DNAPanel bodyEditor;
        // Start is called before the first frame update
        void Start()
        {
            avatarcustomview = GetComponent<AvatarCustomView>();
        }

        public void FaceDNAButtonClick()
        {
            if (orbitor != null)
            {
                TargetFace();
            }
            faceEditor.Initialize(avatar);
            avatarcustomview.HideBodyDNACustomView();
            avatarcustomview.ShowFaceDNACustomView();
        }

        public void CaptureviewButtonClick()
        {
            avatarcustomview.ShowCaptuerView();
        }

        public void BodyDNAButtonClick()
        {
            if (orbitor != null)
            {
                TargetBody();
            }
            bodyEditor.Initialize(avatar);
            avatarcustomview.HideFaceDNACustomView();
            avatarcustomview.ShowBodyNACustomView();
        }

        /// <summary>
        /// Point the mouse orbitor at the body center
        /// </summary>
        public void TargetBody()
        {
            if (orbitor != null)
            {
                orbitor.distance = 1.4f;
                orbitor.TargetBone = UMAMouseOrbitImproved.targetOpts.Chest;
            }
        }

        /// <summary>
        /// Point the mouse orbitor at the neck, so you can see the face.
        /// </summary>
        public void TargetFace()
        {
            if (orbitor != null)
            {
                orbitor.distance = 0.5f;
                orbitor.TargetBone = UMAMouseOrbitImproved.targetOpts.Head;
            }
        }
    }
}