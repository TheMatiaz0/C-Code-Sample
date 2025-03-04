using System;
using UnityEngine;
using UnityEngine.Video;
using Telegraphist.Utils;
using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Router;
using Telegraphist.Helpers.Scenes;
using Telegraphist.Pauseable;
using Telegraphist.UI.Menus;
using Telegraphist.UI.Skippable;
using KBCore.Refs;

namespace Telegraphist.Helpers.Video
{
    [Serializable]
    public class VideoData
    {
        public VideoClip videoClip;
        public LocalizedTextAsset localizedSubtitles;
        
        public static implicit operator bool(VideoData data) => data != null && data.videoClip != null;
    }

    public record VideoSceneArgs(VideoData VideoData, Action Callback) : LoadSceneArgsWithCallback(SceneType.Video, Callback)
    {
        public override string RouteParams => VideoData.videoClip.name;
    }
    
    public class VideoSceneController : SceneEntrypoint<VideoSceneController, VideoSceneArgs>, ISkippable, IPauseable
    {
        [SerializeField] private VideoPlayer player;
        [SerializeField] private SRTSubtitleDisplayer subtitleDisplayer;
        [SerializeField, Scene] private PauseMenuController pauseMenuController;

        private IDisposable skipDisposable;
        private bool completed;

        private void OnEnable()
        {
            pauseMenuController.Register(this);
        }

        private void OnDisable()
        {
            if (pauseMenuController != null)
            {
                pauseMenuController.Unregister(this);
            }
        }

        private void Start()
        {
            LoadVideo();

            player.loopPointReached += OnComplete;

            VersionDisplayer.Current.gameObject.SetActive(false);
        }

        private void LoadVideo()
        {
            var (videoData, onFinish) = SceneArgs;
            
            if (videoData == null || videoData.videoClip == null)
            {
                Debug.LogError("Video scene loaded with null video!");
                onFinish ??= () => GlobalRouter.Current.PopUntilRoot().Forget();
                onFinish();
                return;
            }

            if (videoData.localizedSubtitles != null)
            {
                subtitleDisplayer.SetSubtitleAsset(videoData.localizedSubtitles);
            }
            else
            {
                subtitleDisplayer.gameObject.SetActive(false);
            }

            player.clip = videoData.videoClip;
            player.Play();

            subtitleDisplayer.Run();

            skipDisposable = SkippableController.Current.Register(this);
        }

        private void OnComplete(VideoPlayer source)
        {
            if (completed) return;

            completed = true;
            
            player.loopPointReached -= OnComplete;
            skipDisposable.Dispose();
            source.Stop();
            VersionDisplayer.Current.gameObject.SetActive(true);
            
            SceneArgs.RunCallback();
        }

        public void Skip()
        {
            OnComplete(player);
        }

        public void Pause()
        {
            player.Pause();
        }

        public void Resume()
        {
            player.Play();
        }
    }
}