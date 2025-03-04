using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Scenes;
using Telegraphist.Helpers.Video;

namespace Telegraphist.Helpers.Router.Sequence
{
    public class RouteSequence
    {
        private List<ICallbackInvokable> items;
        private Action callback;
        
        public RouteSequence(params ICallbackInvokable[] items)
        {
            this.items = items.ToList();
        }

        public RouteSequence Add(ICallbackInvokable item)
        {
            items.Add(item);
            return this;
        }
        
        public RouteSequence AddScene(LoadSceneArgsWithCallback args) => Add(new SceneCallbackInvokable(args));

        public RouteSequence AddScene(SceneType scene) => AddScene(new LoadSceneArgsWithCallback(scene, null));
        
        public RouteSequence AddVideoOrSkip(VideoData video)
        {
            if (video)
            {
                return AddScene(new VideoSceneArgs(video, null));
            }

            return Add(new EmptyCallbackInvokable());
        }

        public RouteSequence AddLambda<T>(Action<T, Action> lambda, T arg) => Add(new ActionCallbackInvokable<T>(lambda, arg));
        
        public RouteSequence AddLambda<TResult, TArg>(Func<TArg, Action, TResult> lambda, TArg arg) => Add(new FuncCallbackInvokable<TArg, TResult>(lambda, arg));
        
        public RouteSequence AsReplacement()
        {
            items.Last().IsReplacement = true;

            return this;
        }

        public RouteSequence AsConditional(bool condition)
        {
            if (!condition)
            {
                items[^1] = new EmptyCallbackInvokable();
            }

            return this;
        }

        public RouteSequence WithCallback(Action callback)
        {
            this.callback = callback;

            return this;
        }
        
        public void Run() => RunRecursive(0);

        private void RunRecursive(int i)
        {
            var next = i == items.Count - 1 
                ? callback 
                : () => RunRecursive(i + 1);

            items[i].Run(next).Forget();
        }
    }


}