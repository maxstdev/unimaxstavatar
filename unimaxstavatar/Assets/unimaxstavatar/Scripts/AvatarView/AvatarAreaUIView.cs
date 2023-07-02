using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Maxst.Avatar
{
    public class AvatarAreaUIView : MonoBehaviour
    {
        [SerializeField] private Button closeImg;
        [SerializeField] private Button undoBtn;
        [SerializeField] private Image undoImg;
        [SerializeField] private Button videoBtn;
        [SerializeField] private Button nextBtn;
        [SerializeField] private Button refreshBtn;
        [SerializeField] private Image refreshImg;

        [SerializeField] private Sprite arrowLeftSprite;
        [SerializeField] private Sprite closeBtnSprite;

        [SerializeField] private Sprite undoSprite;
        [SerializeField] private Sprite undoUnselectSprite;

        [SerializeField] private Sprite refreshSprite;
        [SerializeField] private Sprite refreshUnselectSprite;

        public Action OnClickClose;
        public Action OnClickUndo;
        public Action OnClickViedo;
        public Action OnClickNext;
        public Action OnClickRefresh;

        void Start()
        {
            closeImg.
                OnClickAsObservable()
                .Subscribe(_ =>
                {
                    OnClickClose?.Invoke();
                })
                .AddTo(this);

            undoBtn.
                OnClickAsObservable()
                .Subscribe(_ =>
                {
                    OnClickUndo?.Invoke();
                })
                .AddTo(this);

            videoBtn.
                OnClickAsObservable()
                .Subscribe(_ =>
                {
                    OnClickViedo?.Invoke();
                })
                .AddTo(this);

            nextBtn.
                OnClickAsObservable()
                .Subscribe(_ =>
                {
                    OnClickNext?.Invoke();
                })
                .AddTo(this);

            refreshBtn
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    OnClickRefresh?.Invoke();
                })
                .AddTo(this);
        }

        public void SetUndoBtnUI(bool isActive)
        {
            undoImg.sprite = isActive ? undoSprite : undoUnselectSprite;
        }

        public void SetRefreshBtnUI(bool isActive)
        {
            refreshImg.sprite = isActive ? refreshSprite : refreshUnselectSprite;
        }

        public void SetActiveCloseImg(bool isActive)
        {
            if (closeImg.gameObject.activeSelf != isActive)
                closeImg.gameObject.SetActive(isActive);
        }

        public void SetSpriteCloseImg()
        {
            closeImg.GetComponent<Image>().sprite = closeBtnSprite;
        }

        public void SetSpriteArrowLeft()
        {
            closeImg.GetComponent<Image>().sprite = arrowLeftSprite;
        }
    }
}