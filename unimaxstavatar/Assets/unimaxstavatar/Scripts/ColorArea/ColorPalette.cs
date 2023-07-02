using HSVPicker;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Maxst.Avatar
{
    public class ColorPalette : MonoBehaviour
    {
        [SerializeField] private Button confirmBtn;
        [SerializeField] private Button cancelBtn;
        [SerializeField] private ColorPicker colorpicker;
        [SerializeField] private Button bottomSheet;

        public Action OnclickConfirm;
        public Action OnclickCancel;
        public Action OnClickSheet;
        public ReactiveProperty<Color> selectedColor = new ReactiveProperty<Color>();
        public ReactiveProperty<Color> changeColor = new ReactiveProperty<Color>();

        void Start()
        {
            confirmBtn?.onClick.AddListener(() =>
            {
                OnclickConfirm?.Invoke();
            });

            cancelBtn?.onClick.AddListener(() =>
            {
                OnclickCancel?.Invoke();
                colorpicker.CurrentColor = changeColor.Value;
            });

            colorpicker.onValueChanged.AddListener((color) =>
            {
                selectedColor.Value = color;
            });

            changeColor
                .Where(color => color.a > 0f)
                .Subscribe(color =>
                {
                    colorpicker.CurrentColor = color;
                });

            bottomSheet?.onClick.AddListener(() =>
            {
                OnClickSheet?.Invoke();
            });
        }
    }
}