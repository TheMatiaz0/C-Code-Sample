using Cysharp.Threading.Tasks;

namespace Telegraphist.Helpers.Router.Segments
{
    public record TempRouteSegment(IRouteSegment Inner, GlobalRouter Router) : IRouteSegment
    {
        public string RouteName => Inner.RouteName;
        public IRouteSegment Unwrapped => Inner.Unwrapped;
        
        public UniTask OnEnter() => Inner.OnEnter();
        
        public UniTask OnExit() => Inner.OnExit();
        
        public UniTask OnFocus() => Inner.OnFocus();
        
        public UniTask OnBlur()
        {
            Router.RemoveManual(x => ReferenceEquals(x, this), notify: false);
            return Inner.OnBlur();
        }
    }
}