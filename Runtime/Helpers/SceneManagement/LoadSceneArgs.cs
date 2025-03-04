using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Router;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Telegraphist.Helpers.Scenes
{
    public record LoadSceneArgs(SceneType SceneType, bool WithTransition = true, float Delay = 0) : IRouteSegment
    {
        public virtual string RouteName => $"scene:{SceneType}";
        public virtual string RouteParams => null;
    
        public virtual UniTask OnEnter() => SceneLoader.LoadSceneAsync(this);
    
        public virtual UniTask OnExit() => UniTask.CompletedTask;

        public virtual UniTask OnFocus() => SafeFocus(this);

        public virtual UniTask OnBlur() => UniTask.CompletedTask;
        
        public virtual bool CanEnter()
        {
            if (SceneLoader.IsLoading)
            {
                Debug.LogError($"Cannot enter {RouteName} - another scene is already loading");
                return false;
            }

            return true;
        }

        protected UniTask SafeFocus(LoadSceneArgs args)
        {
            if (SceneLoader.IsSceneActive(args.SceneType))
            {
                return UniTask.CompletedTask;
            }
            
            return SceneLoader.LoadSceneAsync(args);
        }

    }
}