using Maxst.Avatar;
using System.Collections.Generic;
using UnityEngine;

public class AvatarRotationController : InjectorBehaviour
{
    [DI(DIScope.singleton)] protected AvatarCustomViewModel avatarCustomViewModel { get; }

    [SerializeField] private Transform target;
    [SerializeField] private Transform light;
    [SerializeField] private float sensitivity = 0.1f;
    private Vector2 turn;
    private float defaultTartgetY = -180f;

    private void Start()
    {
        avatarCustomViewModel.AvatarViewMouseDrag.AddObserver(this, AvatarRotate);
    }

    private void OnDisable()
    {
        avatarCustomViewModel.AvatarViewMouseDrag.RemoveAllObserver(this);
    }


    private void AvatarRotate(Vector2 dragvalue)
    {
        turn.x += dragvalue.x * sensitivity;

        target.localRotation = Quaternion.Euler(0, defaultTartgetY - turn.x, 0);
        light.localRotation = Quaternion.Euler(30, -turn.x, 0);
    }
}
