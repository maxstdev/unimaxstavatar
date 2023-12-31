using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Maxst.Avatar
{
    public class AvatarControlPanelButton : MonoBehaviour
    {
        [SerializeField] private Button faceButton;
        [SerializeField] private Button bodyButton;

        [SerializeField] private GameObject selectFaceIcon;
        [SerializeField] private GameObject selectBodyIcon;

        public ReactiveProperty<ViewType> statecheck = new(ViewType.Face_Eyebrows);

        private void Start()
        {
            selectFaceIcon.SetActive(true);
            selectBodyIcon.SetActive(false);

            faceButton.onClick.AddListener(EnableFaceIcon);
            bodyButton.onClick.AddListener(EnableBodyIcon);

            statecheck
                .Subscribe(value =>
                {
                    switch (value)
                    {
                        case ViewType.Face_Eyebrows:
                        case ViewType.Face_Hair:
                            faceButton.onClick.Invoke();
                            break;
                        case ViewType.Body:
                            bodyButton.onClick.Invoke();
                            break;
                    }
                })
                .AddTo(this);
        }

        public void EnableFaceIcon()
        {
            selectFaceIcon.SetActive(true);
            selectBodyIcon.SetActive(false);
            statecheck.Value = ViewType.Face_Eyebrows;
        }

        public void EnableBodyIcon()
        {
            selectFaceIcon.SetActive(false);
            selectBodyIcon.SetActive(true);
            statecheck.Value = ViewType.Body;
        }
    }
}