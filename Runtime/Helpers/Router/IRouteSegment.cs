using System;
using Cysharp.Threading.Tasks;

namespace Telegraphist.Helpers.Router
{
    public interface IRouteSegment
    {
        string RouteName { get; }
        string RouteParams => null;
        string RouteNameWithParams => $"{RouteName}({RouteParams})";
        IRouteSegment Unwrapped => this;
        bool CanEnter() => true;
        UniTask OnEnter();
        UniTask OnExit();
        UniTask OnFocus();
        UniTask OnBlur();
    }
}