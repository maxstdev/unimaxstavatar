namespace Maxst.Avatar
{
    public class NativeBridge : InjectorBehaviour, INative
    {
        [DI(DIScope.singleton)] protected AvatarCustomViewModel avatarCustomViewModel { get; }

        private void Awake()
        {
            NativeService.Instance.RegisterObserver(this);
        }

        private void OnDestroy()
        {
            NativeService.Instance?.UnregisterObserver(this);
        }

        private void OnEnable()
        {
            avatarCustomViewModel.NativeClose.AddObserver(this, () => { NativeService.Instance.Close(); });
        }

        private void OnDisable()
        {
            avatarCustomViewModel.NativeClose.RemoveAllObserver(this);
        }

        public void Init()
        {
            avatarCustomViewModel.NativeStart.Post();
        }

        public void Close() { }
    }
}

