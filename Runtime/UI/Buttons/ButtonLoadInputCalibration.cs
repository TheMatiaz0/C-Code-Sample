using Cysharp.Threading.Tasks;
using Honey;
using Telegraphist.Helpers;
using Telegraphist.Helpers.Router;
using Telegraphist.Helpers.Router.Segments;
using Telegraphist.Helpers.Scenes;
using Telegraphist.Onboarding.Calibration;
using Telegraphist.Progression;
using UnityEngine;
using UnityEngine.Serialization;

namespace Telegraphist.UI.Buttons
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class ButtonLoadInputCalibration : ActionButton
    {
        [SerializeField] private bool onlyFirstTime = true;
        
        [FormerlySerializedAs("openHubInsteadOfScene")] [SerializeField] 
        private bool openHubAtEnd = false;
        
        protected override void OnButtonClick()
        {
            if (onlyFirstTime && CalibrationController.HasCalibrated)
            {
                Callback();
                return;
            }
            
            GlobalRouter.Current.Push(new CalibrationSceneArgs(Callback)).Forget();
        }

        private void Callback()
        {
            if (openHubAtEnd)
            {
                GameFlow.Current.ResumeFlow();
            }
            else
            {
                GlobalRouter.Current.PopUntilPreviousScene();
            }
        }
    }
}