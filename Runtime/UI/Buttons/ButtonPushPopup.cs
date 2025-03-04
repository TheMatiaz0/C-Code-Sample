using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Router;
using Telegraphist.Helpers.Router.Segments;
using Telegraphist.UI.GenericPopup;
using UnityEngine;

namespace Telegraphist.UI.Buttons
{
    public class ButtonPushPopup : ActionButton
    {
        [SerializeField] private PopupAnimation popup;
        
        protected override void OnButtonClick()
        {
            GlobalRouter.Current.Push(new PopupRouteSegment(popup)).Forget();
        }
    }
}