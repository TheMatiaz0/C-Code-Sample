using Cysharp.Threading.Tasks;
using Telegraphist.Helpers;
using Telegraphist.Helpers.Router;
using Telegraphist.Helpers.Scenes;
using Telegraphist.Input;
using UnityEngine;
using UnityEngine.Video;
using UniRx;

namespace Telegraphist.Helpers.Video
{
    public class IntroVideoController : SceneEntrypoint<IntroVideoController, LoadSceneArgsWithCallback>
    {
        [SerializeField]
        private VideoPlayer player;
        [SerializeField]
        private SceneType changeScene;

        private bool completed;

        private void Start()
        {
            SkippableInputReceiver.Current.OnSkipHeld.Subscribe(_ => OnSkipPressed()).AddTo(this);
            player.loopPointReached += OnComplete;
        }

        private void OnSkipPressed()
        {
            OnComplete(player);
        }

        private void OnComplete(VideoPlayer source)
        {
            if (completed) return;

            completed = true;
            
            player.loopPointReached -= OnComplete;
            source.Stop();

            SceneArgs.RunCallback();
        }
    }
}
