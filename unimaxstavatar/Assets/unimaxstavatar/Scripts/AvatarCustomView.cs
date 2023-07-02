using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maxst.Avatar
{
    public class AvatarCustomView : MonoBehaviour
    {
        [SerializeField] private GameObject faceDNAView;
        [SerializeField] private GameObject bodyDNAView;
        [SerializeField] private GameObject captureView;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void ShowFaceDNACustomView()
        {
            faceDNAView.SetActive(true);
        }

        public void HideFaceDNACustomView()
        {
            faceDNAView.SetActive(false);
        }

        public void ShowBodyNACustomView()
        {
            bodyDNAView.SetActive(true);
        }

        public void HideBodyDNACustomView()
        {
            bodyDNAView.SetActive(false);
        }

        public void ShowCaptuerView()
        {
            captureView.SetActive(true);
        }

        public void HideCaptuerView()
        {
            captureView.SetActive(false);
        }
    }
}