using UnityEngine;
using UnityEngine.EventSystems;

namespace Telegraphist.LevelEditor.Timeline
{
    public class TimelineCursorArea : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private TimelineCursor cursor;

        public void OnPointerClick(PointerEventData eventData)
        {
            cursor.OnAreaClick(eventData.pressPosition.x);
        }

        public void OnDrag(PointerEventData eventData)
        {
            cursor.OnDrag(eventData);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            cursor.OnEndDrag(eventData);
        }
    }
}
