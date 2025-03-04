using System;
using Cysharp.Threading.Tasks;
using Honey;
using Telegraphist.Helpers.Router.Segments;
using Telegraphist.Helpers.Scenes;
using Telegraphist.Helpers.Video;
using UnityEngine;

namespace Telegraphist.Helpers.Router.Sequence
{
    public enum RouteSequenceItemType
    {
        Scene,
        Video,
        PopToRoot,
        PreviousScene,
    }
    
    [Serializable]
    public class RouteSequenceItemSerializable : ICallbackInvokable
    {
        public RouteSequenceItemType type;

        [field:SerializeField] 
        public bool IsReplacement { get; set; }

        [HoneyRun, HShowIf(nameof(IsScene))]
        public SceneType scene;
        
        [HoneyRun, HShowIf(nameof(IsVideo))]
        public VideoData video;
        
        private bool IsScene => type == RouteSequenceItemType.Scene;
        private bool IsVideo => type == RouteSequenceItemType.Video;
        
        public async UniTask Run(Action next)
        {
            if (type == RouteSequenceItemType.PopToRoot)
            {
                await GlobalRouter.Current.PopUntilRoot();
                return;
            }
            if (type == RouteSequenceItemType.PreviousScene)
            {
                await GlobalRouter.Current.PopUntilPreviousScene();
                return;
            }
            
            var args = type switch
            {
                RouteSequenceItemType.Scene => new LoadSceneArgsWithCallback(scene, next),
                RouteSequenceItemType.Video => new VideoSceneArgs(video, next),
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
            
            await GlobalRouter.Current.PushOrReplace(args, IsReplacement);
        }
    }

}