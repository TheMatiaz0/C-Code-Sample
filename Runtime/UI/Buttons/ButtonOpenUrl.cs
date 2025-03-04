using UnityEngine;
using UnityEngine.UI;

namespace Telegraphist.UI.Buttons
{
    [RequireComponent(typeof(Button))]
    public class ButtonOpenUrl : ActionButton
    {
        [field:SerializeField] public string Url { get; set; }
        
        protected override void OnButtonClick()
        {
            Application.OpenURL(Url);
        }
    }
}