using Telegraphist.UI.Settings;
using UnityEngine;

namespace Telegraphist.UI.Buttons
{
    public class ButtonOpenSettings : ActionButton
    {
        [SerializeField]
        private SettingsPopup settingsPopup;
        
        protected override void OnButtonClick()
        {
            settingsPopup.OpenSettingsPopup();            
        }
    }
}
