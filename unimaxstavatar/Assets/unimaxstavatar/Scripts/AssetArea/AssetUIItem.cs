using Maxst.Passport;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Maxst.Avatar
{
    public class AssetUIItem : MonoBehaviour
    {
        [SerializeField] private Button button;

        [SerializeField] private Image thumbnailImg;
        [SerializeField] private GameObject selectedImg;

        public ReactiveProperty<bool> isSelect = new ReactiveProperty<bool>(false);
        public Action OnclickButton;

        private void Start()
        {
            button.
                OnClickAsObservable()
                .Subscribe(_ =>
                {
                    OnclickButton?.Invoke();
                })
                .AddTo(this);

            isSelect
                .Subscribe(value =>
                {
                    selectedImg.SetActive(value);
                })
                .AddTo(this);
        }

        public void SetData(Sprite thumbnail)
        {
            thumbnailImg.sprite = thumbnail;
        }

        public void SetData(string thumbailpath)
        {
            Dictionary<string, string> token = new()
            {
                { "token", "Bearer " + TokenRepo.Instance.GetToken().accessToken }
            };

            Davinci.get().load(thumbailpath).setFadeTime(0)
                        .SetHeaders(token)
                        .into(thumbnailImg).setCached(true).start();
        }
    }
}