using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Router.Segments;
using Telegraphist.Helpers.Scenes;
using Telegraphist.Lifecycle;
using UnityEngine;

namespace Telegraphist.Helpers.Router
{
    public class RouterRootScene : LifetimeSingleton<RouterRootScene>
    {
        [SerializeField] private SceneType rootScene = SceneType.MainMenu;
        [SerializeField] private GlobalRouter router;

#if UNITY_EDITOR
        private static IRouteSegment initialRoute;
#endif

        public override void Setup()
        {
            base.Setup();

            router.PushManual(new LoadSceneArgs(rootScene));

#if UNITY_EDITOR
            if (initialRoute != null)
            {
                router.Push(initialRoute).Forget();
                initialRoute = null;
                return;
            }
#endif

            var activeScene = SceneLoader.GetActiveScene();
            if (activeScene != rootScene)
            {
                router.PushManual(new LoadSceneArgs(activeScene));
            }
        }

#if UNITY_EDITOR
        public static void EditorSetInitialRoute(IRouteSegment segment)
        {
            initialRoute = segment;
        }
#endif
    }
}