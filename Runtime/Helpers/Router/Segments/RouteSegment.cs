using System;
using Cysharp.Threading.Tasks;

namespace Telegraphist.Helpers.Router.Segments
{
    public record RouteSegment(
        string RouteName,
        Func<UniTask> OnEnter,
        Func<UniTask> OnExit,
        Func<UniTask> OnFocus,
        Func<UniTask> OnBlur) : IRouteSegment
    {
        UniTask IRouteSegment.OnEnter() => OnEnter();

        UniTask IRouteSegment.OnExit() => OnExit();

        UniTask IRouteSegment.OnFocus() => OnFocus();

        UniTask IRouteSegment.OnBlur() => OnBlur();
    }
}