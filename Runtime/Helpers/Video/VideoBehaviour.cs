using UnityEngine;
using UnityEngine.Video;

namespace Telegraphist.Helpers.Video
{
    public class VideoBehaviour : MonoBehaviour
    {
        [SerializeField]
        private VideoPlayer videoPlayer;

        private VideoClip videoClip;

        private void Start()
        {
            if (videoPlayer == null)
            {
                Debug.LogError("NO video player component found");
            }
            videoClip = videoPlayer.clip;
            OnTargetFound();
        }

        private void OnDestroy()
        {
            videoPlayer.prepareCompleted -= OnPreperationComplete;
            videoPlayer.errorReceived -= OnError;
        }

        private void OnTargetFound()
        {
            if (videoPlayer.renderMode == VideoRenderMode.RenderTexture)
            {
                videoPlayer.targetTexture.Release();
            }
            videoPlayer.clip = videoClip;
            videoPlayer.Prepare();
            videoPlayer.errorReceived += OnError;
            videoPlayer.prepareCompleted += OnPreperationComplete;
        }

        private void OnError(VideoPlayer source, string message)
        {
            Debug.LogError(message);
        }

        private void OnPreperationComplete(VideoPlayer vPlayer)
        {
            vPlayer.Play();
        }
    }
}
