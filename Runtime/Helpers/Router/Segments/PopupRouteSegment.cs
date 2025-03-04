using Cysharp.Threading.Tasks;
using Telegraphist.UI.GenericPopup;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Telegraphist.Helpers.Router.Segments
{
    public record PopupRouteSegment(PopupAnimation Popup) : IRouteSegment
    {
        public string RouteName => $"popup:{Popup.gameObject.name}";
        
        public async UniTask OnEnter() => Popup.ActivatePopup();

        public async UniTask OnExit() => Popup.DeactivatePopup();

        public async UniTask OnFocus() => Popup.ActivatePopup();

        public UniTask OnBlur() => UniTask.CompletedTask;
    }
}