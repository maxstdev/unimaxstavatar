using System.Collections;
using System.Collections.Generic;
using UMA.CharacterSystem;
using UnityEngine;

namespace Maxst.Avatar
{
    public class SetUpAvatarColor : ICommand
    {
        private DynamicCharacterAvatar avatar;

        private string slotname;
        private Color previousColor;

        private ColorPreset preset;
        private List<ColorPreset> list;
        private List<ColorPreset> previousList = new List<ColorPreset>();

        public SetUpAvatarColor(DynamicCharacterAvatar avater, string slotname, ColorPreset preset, List<ColorPreset> list)
        {
            avatar = avater;
            this.slotname = slotname;
            this.preset = preset;
            this.list = list;
        }

        public void Execute()
        {
            previousColor = avatar.GetColor(slotname).channelMask[0];
            previousList.AddRange(list);

            list.Add(preset);

            avatar.SetColor(slotname, preset.selectColor.Value);
            avatar.UpdateColors(true);
        }

        public void Undo()
        {
            list.ForEach(preset =>
            {
                preset.isSelect.Value = false;
            });
            list.Clear();
            list.AddRange(previousList);

            if (list.Count > 0)
            {
                list[list.Count - 1].isSelect.Value = true;
            }

            avatar.SetColor(slotname, previousColor);
            avatar.UpdateColors(true);
        }
    }
}