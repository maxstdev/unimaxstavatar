using UMA.CharacterSystem;
using UnityEngine;

namespace Maxst.Avatar
{
    public class ActionList : MonoBehaviour
    {
        [SerializeField] private ActionItem defaultButton;
        [SerializeField] private DynamicCharacterAvatar avatar;
        [SerializeField] private GameObject content;
        private ActionItem selectBtn;

        public void SetSelectBtn(ActionItem btn)
        {
            selectBtn = btn;
        }

        public ActionItem GetSelectBtn()
        {
            return selectBtn;
        }
        public void RunAvatarAnimation(string animationType)
        {
            if (avatar != null)
            {
                avatar.GetComponent<Animator>().Play(animationType);
            }
        }

        public void ResetActionList()
        {
            if (selectBtn != null)
            {
                selectBtn.UnSelectButtonUI();
            }
            content.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            defaultButton.SelectButtonUI();
        }
    }
}