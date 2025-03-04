using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Telegraphist.UI
{
    public class SelectableOutline : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private Selectable selectable;
        private Outline outline;
        
        private void Awake()
        {
            selectable = GetComponent<Selectable>();
            outline = GetComponent<Outline>();
            outline.enabled = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!outline) return;
            
            outline.enabled = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!outline) return;
            
            outline.enabled = false;
        }
    }
}