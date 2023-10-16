using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Maxst.Avatar
{
    public class AssetAreaUIView : MonoBehaviour
    {
        [SerializeField] private AssetUIItem assetitemPrefab;
        [SerializeField] private ScrollRect assetScrollview;
        private List<AssetUIItem> assetItemList = new List<AssetUIItem>();

        public Subject<string> slotname = new Subject<string>();
        public ReactiveProperty<ViewType> viewType = new ReactiveProperty<ViewType>();


        public void CreateAssetItem(string slotName, Sprite thumbnail, bool isSelected)
        {
            AssetUIItem item = Instantiate(assetitemPrefab, assetScrollview.content);
            item.SetData(thumbnail);
            item.OnclickButton = () =>
            {
                itemSelectChange(item);
                slotname.OnNext(slotName);
            };
            item.isSelect.Value = isSelected;

            assetItemList.Add(item);
        }

        public void CreateAssetItem(string slotName, string thumnailpath, bool isSelected)
        {
            AssetUIItem item = Instantiate(assetitemPrefab, assetScrollview.content);
            item.SetData(thumnailpath);
            item.OnclickButton = () =>
            {
                itemSelectChange(item);
                slotname.OnNext(slotName);
            };
            item.isSelect.Value = isSelected;

            assetItemList.Add(item);
        }

        public void DeleteAllItem()
        {
            foreach (var asset in assetItemList)
            {
                Destroy(asset.gameObject);
            }
            assetItemList.Clear();
        }

        private void itemSelectChange(AssetUIItem item)
        {
            foreach (var asset in assetItemList)
            {
                asset.isSelect.Value = false;
            }
            item.isSelect.Value = true;
        }
    }
}