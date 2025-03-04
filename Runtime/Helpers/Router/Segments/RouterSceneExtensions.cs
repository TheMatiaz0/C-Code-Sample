using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Scenes;

namespace Telegraphist.Helpers.Router.Segments
{
    public static class RouterSceneExtensions
    {
        public static UniTask Push(this GlobalRouter router, SceneType sceneType) =>
            router.Push(new LoadSceneArgs(sceneType));
        
        public static UniTask Replace(this GlobalRouter router, SceneType sceneType) =>
            router.Replace(new LoadSceneArgs(sceneType));
        
        public static UniTask PopUntilScene(this GlobalRouter router, SceneType sceneType) => 
            router.PopUntil(item => item.Unwrapped is LoadSceneArgs sceneItem && sceneItem.SceneType == sceneType);
        
        public static UniTask PopUntilSceneAndReplace(this GlobalRouter router, SceneType sceneType, IRouteSegment segment) => 
            router.PopUntilAndReplace(item => item.Unwrapped is LoadSceneArgs sceneItem && sceneItem.SceneType == sceneType, segment);
        
        public static UniTask PopUntilPreviousScene(this GlobalRouter router) =>
            router.PopUntil(item => item.Unwrapped is LoadSceneArgs sceneItem && !SceneLoader.IsSceneActive(sceneItem.SceneType));
    }
}