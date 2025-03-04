using System;
using Cysharp.Threading.Tasks;

namespace Telegraphist.Helpers.Router.Sequence
{
    public interface ICallbackInvokable
    {
        bool IsReplacement { get; set; }
        UniTask Run(Action next);
    }
}