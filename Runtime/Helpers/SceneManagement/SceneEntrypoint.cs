using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Router;
using Telegraphist.Lifecycle;
using UnityEngine;

namespace Telegraphist.Helpers.Scenes
{
    public abstract class SceneEntrypoint<T, TArgs> : SceneSingleton<T> 
        where T : SceneEntrypoint<T, TArgs> 
        where TArgs : LoadSceneArgs
    {
        protected TArgs SceneArgs { get; private set; }

        protected virtual TArgs DefaultSceneArgs => null;
        protected virtual UniTask<TArgs> GetDefaultSceneArgsAsync() => UniTask.FromResult(DefaultSceneArgs);

        protected override void Awake()
        {
            base.Awake();
            
            UniTask.Void(async () =>
            {
                SceneArgs = SceneLoader.GetSceneArguments<TArgs>() ?? await GetDefaultSceneArgsAsync();

                if (SceneArgs == null)
                {
                    Debug.LogError($"Scene {GetType().Name} was loaded without arguments and no default arguments have been provided", this);
                    GlobalRouter.Current.PopUntilRoot().Forget();
                    return;
                }
            
                InitScene();
            });
        }

        protected virtual void InitScene()
        {
        }
    }
}