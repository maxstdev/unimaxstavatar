using System;
using System.Collections.Generic;
using System.Linq;
using UMA.CharacterSystem;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Maxst.Avatar
{
    [Serializable]
    public class PresetColorData
    {
        public List<Color> colors;
    }

    public class ColorAreaUIView : MonoBehaviour
    {
        [SerializeField] private DynamicCharacterAvatar avatar;

        private const string PRESET_COLOR_KEY = "PRESET_COLOR_KEY";
        private const int maxpresetcount = 10;

        [SerializeField] private ColorPalette palette;
        [SerializeField] private Button palettebutton;

        private ReactiveCollection<ColorPreset> colorPresetList = new ReactiveCollection<ColorPreset>();
        [SerializeField] private ColorPreset presetPrefab;
        [SerializeField] private ScrollRect presetScrollview;
        private Color initcolor;

        public Func<Color> getDefaultColor;
        public ReactiveProperty<Color> previewcolor = new ReactiveProperty<Color>();
        public ReactiveProperty<ColorPreset> setupColorpreset = new ReactiveProperty<ColorPreset>();

        [SerializeField] private List<Color> defaultColors = new List<Color>();

        private Color SelectAvatarHairColorUI()
        {
            return avatar.characterColors.Colors.FirstOrDefault(each => each.name.Equals("Hair") && defaultColors.Contains(each.color))?.color ?? default;
        }

        private void OnEnable()
        {
            palettebutton.onClick.RemoveAllListeners();
            palettebutton.onClick.AddListener(() =>
            {
                initcolor = getDefaultColor();
                palette.changeColor.SetValueAndForceNotify(initcolor);
                ShowPalette();
            });

            CreateDefaultPreset();
        }

        private void ShowPalette()
        {
            palette.gameObject.SetActive(true);
            PaletteInit();
        }

        private void PaletteInit()
        {
            palette.OnclickConfirm = () =>
            {
                var preset = CreateColorPreset(palette.selectedColor.Value);
                AddList(preset);
                preset.OnclickButton?.Invoke();

                palette.gameObject.SetActive(false);
            };

            palette.OnclickCancel = ClosePalette;
            palette.OnClickSheet = ClosePalette;

            palette.selectedColor
                .Where(value => value.a > 0f)
                .Subscribe(value =>
                {
                    previewcolor.Value = value;
                })
                .AddTo(this);
        }

        private void CreateDefaultPreset()
        {
            var defaultdata = GetColorPresetValue();

            //var selectColor = SelectAvatarHairColorUI();

            if (defaultdata != null)
            {
                foreach (var color in defaultdata.colors)
                {
                    var preset = CreateColorPreset(color);
                    //if (selectColor == color) preset.isSelect.Value = true;    
                    AddList(preset);
                }
            }
        }

        private ColorPreset CreateColorPreset(Color color)
        {
            var colorpreset = Instantiate(presetPrefab, presetScrollview.content);
            colorpreset.SetPresetColor(color);

            colorpreset.OnclickButton = () =>
            {
                setupColorpreset.SetValueAndForceNotify(colorpreset);
                //PresetSelectChange(colorpreset);
            };

            return colorpreset;
        }

        private void AddList(ColorPreset preset)
        {
            colorPresetList.Add(preset);

            RemoveLastpreset();
            SetColorPresetValue();
        }

        private void RemoveLastpreset()
        {
            if (colorPresetList.Count > maxpresetcount)
            {
                var last = colorPresetList[0];
                Destroy(last.gameObject);
                colorPresetList.Remove(last);
            }
        }

        private void OnDisable()
        {
            DeleteAllPreset();
            ScrollPositionInit();
        }

        private void DeleteAllPreset()
        {
            foreach (var preset in colorPresetList)
            {
                Destroy(preset.gameObject);
            }
            colorPresetList.Clear();
        }

        private void ScrollPositionInit()
        {
            presetScrollview.horizontalNormalizedPosition = 0;
        }

        private void PresetSelectChange(ColorPreset preset)
        {
            foreach (var value in colorPresetList)
            {
                value.isSelect.Value = false;
            }

            preset.isSelect.Value = true;
        }

        private void ClosePalette()
        {
            palette.gameObject.SetActive(false);
            previewcolor.SetValueAndForceNotify(initcolor);
        }

        #region GET/SET ColorData

        private string DefautColorData()
        {
            var colordata = new PresetColorData();

            colordata.colors = defaultColors;

            return JsonUtility.ToJson(colordata);
        }

        private PresetColorData GetColorPresetValue()
        {
            var colordatas = PlayerPrefs.GetString(PRESET_COLOR_KEY, DefautColorData());
            var data = JsonUtility.FromJson<PresetColorData>(colordatas);

            return data;
        }

        private void SetColorPresetValue()
        {
            var colordata = new PresetColorData();
            colordata.colors = new List<Color>();

            foreach (var value in colorPresetList)
            {
                colordata.colors.Add(value.selectColor.Value);
            }

            PlayerPrefs.SetString(PRESET_COLOR_KEY, JsonUtility.ToJson(colordata));
        }

        #endregion
    }
}