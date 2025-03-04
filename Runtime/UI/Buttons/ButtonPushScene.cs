using Cysharp.Threading.Tasks;
using Telegraphist.Helpers;
using Telegraphist.Helpers.Router;
using Telegraphist.Helpers.Router.Segments;
using Telegraphist.Helpers.Scenes;
using UnityEngine;

namespace Telegraphist.UI.Buttons
{
    public class ButtonPushScene : ActionButton
    {
        [field:SerializeField] public SceneType SceneType { get; set; }
        
        protected override void OnButtonClick()
        {
            GlobalRouter.Current.Push(new LoadSceneArgs(SceneType)).Forget();
        }
    }
}