using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Maxst.Avatar
{
    public class CategorySubject : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TMPro.TextMeshProUGUI subjectName;

        private Sprite defaultIcon;
        private Sprite selectIcon;

        [SerializeField] private Color defaultColor;
        [SerializeField] private Color selectColor;

        public ReactiveProperty<bool> isSelect = new ReactiveProperty<bool>(false);

        public Action OnclickButton;

        private void Start()
        {
            button.
                OnClickAsObservable()
                .Subscribe(_ =>
                {
                    OnclickButton?.Invoke();
                });

            isSelect
                .Subscribe(value =>
                {
                    if (value)
                    {
                        icon.sprite = selectIcon;
                        subjectName.color = selectColor;
                    }
                    else
                    {
                        icon.sprite = defaultIcon;
                        subjectName.color = defaultColor;
                    }
                });
        }

        public void SetSubject(string name, Sprite defaulticon, Sprite selecticon)
        {
            subjectName.text = name;
            defaultIcon = defaulticon;
            selectIcon = selecticon;
        }
    }
}