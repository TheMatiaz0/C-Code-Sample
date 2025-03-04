using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Telegraphist.LevelEditor.Timeline
{
    public class TimelineScrollRect : ScrollRect
    {
        // disable dragging
        public override void OnBeginDrag(PointerEventData eventData) { }
        public override void OnDrag(PointerEventData eventData) { }
        public override void OnEndDrag(PointerEventData eventData) { }
        
        
        public override void OnScroll(PointerEventData data)
        {
            // Zoom the rect in or out
            if (LevelEditorInputReceiver.Current.ZoomScrollAxis != 0)
            {
                LevelEditor.Timeline.Timeline.Current.ChangeZoom(LevelEditorInputReceiver.Current.ZoomScrollAxis);
                return;
            }
            
            // Change the beat fraction up or down
            if (LevelEditorInputReceiver.Current.BeatFractionScrollAxis != 0)
            {
                LevelEditor.Timeline.Timeline.Current.OnBeatFractionScroll(LevelEditorInputReceiver.Current.BeatFractionScrollAxis);
                return;
            }

            base.OnScroll(data);
        }
    }
}
