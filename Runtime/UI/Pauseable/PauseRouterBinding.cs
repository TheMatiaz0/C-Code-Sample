using System;
using Telegraphist.Helpers.Router;
using Telegraphist.Helpers.Scenes;
using Telegraphist.Pauseable;
using UniRx;
using UnityEngine;

namespace Telegraphist.UI.Menus
{
    public class PauseRouterBinding : MonoBehaviour, IPauseable
    {
        private RouterBackHandler routerBackHandler;
        private GlobalRouter router;
        private IDisposable backHandlerListener;
        
        private void Start()
        {
            router = GlobalRouter.Current;
            routerBackHandler = RouterBackHandler.Current;
            
            // if a scene has pause menu it means ESC should initially open pause instead of going back
            routerBackHandler.IsLocked = true;
            
            PauseMenuController.Current.Register(this);
        }

        private void OnDestroy()
        {
            PauseMenuController.Current.Unregister(this);
        }

        public void Pause()
        {
            routerBackHandler.SetIsLockedWithSmartDelay(false);
            
            // last pop should close pause instead of going back
            backHandlerListener = routerBackHandler.OnBeforeBack
                .Subscribe(_ => 
                {
                    if (router.CurrentSegment.Unwrapped is LoadSceneArgs)
                    {
                        routerBackHandler.IsLocked = true;
                        
                        PauseMenuController.Current.ClosePause();
                    }
                });
        }

        public void Resume()
        {
            routerBackHandler.SetIsLockedWithSmartDelay(true);
            backHandlerListener?.Dispose();
        }
    }
}