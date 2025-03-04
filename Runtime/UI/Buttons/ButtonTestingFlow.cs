using Telegraphist.Onboarding;
using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.UI.Buttons
{
    [RequireComponent(typeof(Button))]
    public class ButtonTestingFlow : ActionButton
    {
        [SerializeField] private TestingStage targetStage;

        protected override void OnButtonClick()
        {
            TestingFlow.Current.ChangeStage(targetStage);
        }
    }
}