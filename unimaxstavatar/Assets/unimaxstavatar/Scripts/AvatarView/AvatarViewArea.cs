using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Maxst.Avatar
{
    public class AvatarViewArea : InjectorBehaviour, IDragHandler, IScrollHandler
    {
        [DI(DIScope.singleton)] protected AvatarCustomViewModel avatarCustomViewModel { get; }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            avatarCustomViewModel.AvatarViewMouseDrag.DirectInvoke(eventData.delta);
        }

        public void OnScroll(PointerEventData eventData)
        {
            //avatarCustomViewModel.AvatarViewMouseSroll.DirectInvoke(eventData.scrollDelta);
        }
    }
}