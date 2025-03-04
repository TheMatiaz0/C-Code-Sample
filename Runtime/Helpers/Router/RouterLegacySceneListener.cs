using System.Linq;
using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Router.Segments;
using Telegraphist.Helpers.Scenes;
using Telegraphist.Lifecycle;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Telegraphist.Helpers.Router
{
    // TODO remove this if all LoadScene calls are replaced with Router.{Push,Replace}Async()
    public class RouterLegacySceneListener : LifetimeSingleton<RouterLegacySceneListener>
    {
        [SerializeField] private GlobalRouter router;
        
        public override void Setup()
        {
            base.Setup();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single) return;
            
            var sceneType = SceneLoader.GetSceneType(scene);
            
            if (router.CurrentSegment.Unwrapped is LoadSceneArgs sceneArgs && sceneArgs.SceneType == sceneType)
            {
                return;
            }
            
            Debug.LogWarning("Scene loaded outside of Router! Fix this to avoid unexpected behaviour.", this);
            router.PushManual(new LoadSceneArgs(SceneLoader.GetSceneType(scene)));
        }
    }
}