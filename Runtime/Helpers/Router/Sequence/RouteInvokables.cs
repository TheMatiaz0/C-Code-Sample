using System;
using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Scenes;

namespace Telegraphist.Helpers.Router.Sequence
{
    public record ActionCallbackInvokable<T>(Action<T, Action> Lambda, T Arg0) : ICallbackInvokable
    {
        public bool IsReplacement { get; set; }

        public async UniTask Run(Action next) => Lambda(Arg0, next);
    }
    
    public record FuncCallbackInvokable<TArg, TResult>(Func<TArg, Action, TResult> Lambda, TArg Arg) : ICallbackInvokable
    {
        public bool IsReplacement { get; set; }

        public async UniTask Run(Action next) => Lambda(Arg, next);
    }

    public class EmptyCallbackInvokable : ICallbackInvokable
    {
        public bool IsReplacement { get; set; }
        
        public async UniTask Run(Action next) => next();
    }
    
    public record SceneCallbackInvokable(LoadSceneArgsWithCallback Args) : ICallbackInvokable
    {
        public bool IsReplacement { get; set; }
        
        public async UniTask Run(Action next) => 
            await GlobalRouter.Current.PushOrReplace(Args with { Callback = next }, IsReplacement);
    }
}