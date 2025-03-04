using System;
using Cysharp.Threading.Tasks;
using Telegraphist.Lifecycle;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Telegraphist.Helpers.Router
{
    public class RouterBackHandler : SceneSingleton<RouterBackHandler> // ensure only one on scene
    {
        [SerializeField] private InputActionReference cancelReference;
        
        public bool IsLocked { get; set; }
        
        public bool IsActive => !IsLocked && isActiveAndEnabled;

        public IObservable<Unit> OnBeforeBack => onBeforeBack;

        private Subject<Unit> onBeforeBack = new();
        
        private void Start()
        {
            cancelReference.action.started += OnCancel;
        }

        private void OnDestroy()
        {
            cancelReference.action.started -= OnCancel;
        }

        private void OnCancel(InputAction.CallbackContext obj)
        {
            if (!IsActive) return;
            
            onBeforeBack.OnNext(Unit.Default);

            if (!IsActive) return;
            
            GlobalRouter.Current.Pop().Forget();
        }

        public void SetIsLockedWithSmartDelay(bool isLocked)
        {
            if (isLocked)
            {
                // locking should happen immediately
                IsLocked = true;
            }
            else
            {
                // unlocking should be delayed to prevent double event handling
                UniTask.Void(async () =>
                {
                    await UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate);
                    IsLocked = false;
                });
            }
        }
    }
}