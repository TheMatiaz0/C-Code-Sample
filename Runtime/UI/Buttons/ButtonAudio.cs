using UnityEngine;
using UnityEngine.EventSystems;

namespace Telegraphist.UI.Buttons
{
    public class ButtonAudio : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        private UnityEngine.UI.Button btn;

        [SerializeField]
        private AudioSource source;

        [SerializeField]
        private AudioClip hoverSound;

        [SerializeField]
        private AudioClip pressedSound;

        private void Awake()
        {
            btn = GetComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            source.PlayOneShot(pressedSound);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            source.PlayOneShot(hoverSound);
        }

        public void OnSelect(BaseEventData eventData)
        {
            source.PlayOneShot(hoverSound);
        }
    }
}
