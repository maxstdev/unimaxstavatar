using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Maxst.Avatar
{
    public class ActionItem : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private ActionList actionList;
        [SerializeField] private ActionItem btn;
        [SerializeField] private string animationType;
        public void OnSelect(BaseEventData eventData)
        {
            var beforeSelectButton = actionList.GetSelectBtn();
            if (beforeSelectButton != null && beforeSelectButton != eventData.selectedObject.gameObject)
            {
                UnSelectButtonUI();
            }
            SelectButtonUI();
        }

        public string GetAnimationType()
        {
            return animationType;
        }

        public void UnSelectButtonUI()
        {
            var beforeSelectButton = actionList.GetSelectBtn();
            foreach (var each in beforeSelectButton.GetComponentsInChildren<Outline>()) each.enabled = false;
            beforeSelectButton.GetComponentInChildren<TextMeshProUGUI>().color = ColorsSO.Object.Gray700;
        }

        public void SelectButtonUI()
        {
            foreach (var each in btn.GetComponentsInChildren<Outline>()) each.enabled = true;
            btn.GetComponentInChildren<TextMeshProUGUI>().color = ColorsSO.Object.Primary;

            actionList.SetSelectBtn(btn);
            actionList.RunAvatarAnimation(animationType);
        }
    }
}