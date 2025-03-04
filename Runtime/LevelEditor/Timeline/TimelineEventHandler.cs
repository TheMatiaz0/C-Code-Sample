using Telegraphist.LevelEditor.Timeline.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Telegraphist.LevelEditor.Timeline
{
    public class TimelineEventHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Timeline timeline;
        [SerializeField] private TimelineToolsController toolsController;
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            var timelinePosition = timeline.PointerEventToGridPositionInt(eventData);
            toolsController.ActiveTool.OnBeginDrag(eventData, timelinePosition);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var timelinePosition = timeline.PointerEventToGridPositionInt(eventData);
            toolsController.ActiveTool.OnDrag(eventData, timelinePosition);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var timelinePosition = timeline.PointerEventToGridPositionInt(eventData);
            toolsController.ActiveTool.OnEndDrag(eventData, timelinePosition);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            var timelinePosition = timeline.PointerEventToGridPositionInt(eventData);
            toolsController.ActiveTool.OnPointerDown(eventData, timelinePosition);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            var timelinePosition = timeline.PointerEventToGridPositionInt(eventData);
            toolsController.ActiveTool.OnPointerUp(eventData, timelinePosition);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            var timelinePosition = timeline.PointerEventToGridPositionInt(eventData);
            toolsController.ActiveTool.OnPointerClick(eventData, timelinePosition);
        }
    }
}
