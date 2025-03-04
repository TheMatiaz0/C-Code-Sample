using Cysharp.Threading.Tasks;
using Telegraphist.Helpers.Router;

namespace Telegraphist.UI.Buttons
{
    public class ButtonPopUntilRoot : ActionButton
    {
        protected override void OnButtonClick()
        {
            GlobalRouter.Current.PopUntilRoot().Forget();
        }
    }
}