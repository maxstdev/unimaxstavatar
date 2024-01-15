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
        private Dictionary<string, AssetUIItem> assetItemDictionary = new();

        public Subject<string> slotname = new Subject<string>();
        public ReactiveProperty<ViewType> viewType = new ReactiveProperty<ViewType>();


        public void CreateAssetItem(string slotName, Sprite thumbnail, bool isSelected)
        {
            AssetUIItem item = Instantiate(assetitemPrefab, assetScrollview.content);
            item.SetData(thumbnail);
            item.OnclickButton = () =>
            {
                ItemSelectChange(item);
                slotname.OnNext(slotName);
            };
            item.isSelect.Value = isSelected;

            assetItemDictionary.Add(slotName, item);
        }

        public AssetUIItem CreateAssetItem(string slotName, bool isSelected) {
            AssetUIItem item = Instantiate(assetitemPrefab, assetScrollview.content);
            
            item.OnclickButton = () =>
            {
                ItemSelectChange(item);
                slotname.OnNext(slotName);
            };
            item.isSelect.Value = isSelected;

            assetItemDictionary.Add(slotName, item);
            return item;
        }
        public void CreateAssetItem(string slotName, string thumnailpath, bool isSelected)
        {
            AssetUIItem item = Instantiate(assetitemPrefab, assetScrollview.content);
            item.SetData(thumnailpath);
            item.OnclickButton = () =>
            {
                ItemSelectChange(item);
                slotname.OnNext(slotName);
            };
            item.isSelect.Value = isSelected;

            assetItemDictionary.Add(slotName, item);
        }

        public void DeleteAllItem()
        {
            foreach(var asset in assetItemDictionary.Values)
            {
                Destroy(asset.gameObject);
            }
            assetItemDictionary.Clear();
        }

        public void ItemSelectChange(string slotName)
        {
            assetItemDictionary.TryGetValue(slotName, out var item);
            if (item != null)
            {
                ItemSelectChange(item);
            }
            else {
                Debug.LogError("[AssetAreaUIView] ItemSelectChange : No item found.");
            }
        }

        public void ItemSelectChange(AssetUIItem item = null)
        {
            InitSelect();

            if (item != null)
            {
                item.isSelect.Value = true;
            }
        }

        public void InitSelect() {
            foreach (var asset in assetItemDictionary.Values)
            {
                asset.isSelect.Value = false;
            }
        }

        public AssetUIItem GetItem(string slotname)
        {
            return assetItemDictionary[slotname];
        }
    }
}