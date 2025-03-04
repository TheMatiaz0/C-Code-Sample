using Cysharp.Threading.Tasks;
using Telegraphist.Helpers;
using Telegraphist.Helpers.Router;
using Telegraphist.Helpers.Router.Segments;
using Telegraphist.Helpers.Scenes;
using UnityEngine;

namespace Telegraphist.UI.Buttons
{
    public class ButtonReplaceScene : ActionButton
    {
        [SerializeField] private SceneType sceneType;
        [SerializeField] private bool popAllBefore;
        
        protected override void OnButtonClick()
        {
            if (popAllBefore)
            {
                GlobalRouter.Current.ReplaceTopLevel(new LoadSceneArgs(sceneType)).Forget();
            }
            else
            {
                GlobalRouter.Current.Replace(new LoadSceneArgs(sceneType)).Forget();
            }
        }
    }
}