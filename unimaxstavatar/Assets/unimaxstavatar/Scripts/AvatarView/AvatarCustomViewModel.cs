using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using MaxstUtils;

namespace Maxst.Avatar
{
    public class AvatarCustomViewModel
    {
        protected AvatarCustomViewModel()
        {
            raceDataWardrobeSlotList = new List<string>();
        }

        public LiveEvent<string, Color> onChangeColorValue = new LiveEvent<string, Color>();
        public LiveEvent<Color> currentAvatarColor = new LiveEvent<Color>();

        public LiveEvent<bool> endSetUpPartsItem = new LiveEvent<bool>();
        public LiveEvent endCreateSlotItem = new LiveEvent();
        public LiveEvent endCreateColorItem = new LiveEvent();

        public LiveEvent<Sprite> changePartsitemThumbnail = new LiveEvent<Sprite>();

        public LiveEvent<Vector2> AvatarViewMouseSroll = new LiveEvent<Vector2>();
        public LiveEvent<Vector2> AvatarViewMouseDrag = new LiveEvent<Vector2>();
        public LiveEvent<ViewType> ViewTypeLiveEvent = new();

        public LiveEvent completeButtonClick = new LiveEvent();
        public LiveEvent saveButtonClick = new LiveEvent();
        public LiveEvent loadButtonClick = new LiveEvent();
        public LiveEvent CaptureExecute = new LiveEvent();

        public LiveEvent NativeClose = new LiveEvent();
        public LiveEvent NativeStart = new LiveEvent();

        public List<string> raceDataWardrobeSlotList { get; }

        public string GetSavePath()
        {
            var path = $"{Application.dataPath}/SubModule/20.SaveData/";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;

        }
    }
}