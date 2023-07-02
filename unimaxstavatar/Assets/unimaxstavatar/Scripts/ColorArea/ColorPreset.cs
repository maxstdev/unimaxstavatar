using HSVPicker;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Maxst.Avatar
{
    public class ColorPreset : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject inner;
        [SerializeField] private GameObject outer;
        [SerializeField] private GameObject defalut;
        [SerializeField] private GameObject select;

        public ReactiveProperty<Color> selectColor = new ReactiveProperty<Color>();
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

            selectColor
                .DistinctUntilChanged()
                .Subscribe(color =>
                {
                    inner.GetComponent<Image>().color = color;
                    outer.GetComponent<Image>().color = color;
                    defalut.GetComponent<Image>().color = color;
                })
                .AddTo(this);

            isSelect
                .Subscribe(value =>
                {
                    //select?.SetActive(value);
                    /*
                    inner.SetActive(value);
                    outer.SetActive(value);
                    defalut.SetActive(!value);
                    */
                    SetSelectUI(value);
                })
                .AddTo(this);
        }

        public void SetSelectUI(bool isActive)
        {
            inner.SetActive(isActive);
            outer.SetActive(isActive);
            defalut.SetActive(!isActive);
        }

        public void ActiveFirstItem()
        {
            //inner.SetActive(true);
            //outer.SetActive(true);
            //defalut.SetActive(false);
        }

        public void SetPresetColor(Color value)
        {
            selectColor.Value = value;
        }
    }
}