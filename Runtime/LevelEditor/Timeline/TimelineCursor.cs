using Telegraphist.LevelEditor.Playback;
using Telegraphist.Lifecycle;
using Telegraphist.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Telegraphist.LevelEditor.Timeline
{
    public class TimelineCursor : SongSingleton<TimelineCursor>, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform cursorCue;
        [SerializeField] private Timeline timeline;
        [SerializeField] private ScrollRect scrollRect;

        public float CursorPositionX => cursorCue.anchoredPosition.x;
        public float CursorTime => timeline.GetTimeFromTimelinePosition(CursorPositionX);
        
        private bool dragAutoplay;
        private float lastSongTimePosition = 0;
        
        public void UpdateBoundaries(bool isDragging)
        {
            ClampContentEdges();
            ClampViewportEdges(isDragging);
        }

        private void ClampContentEdges()
        {
            if (cursorCue.anchoredPosition.x < timeline.GetTimelinePositionForTime(0))
            {
                cursorCue.position = new(timeline.GetTimelinePositionForTime(0), cursorCue.position.y);
            }
            else if (cursorCue.anchoredPosition.x > timeline.GetTimelinePositionForTime(LevelEditorContext.Current.SongAudio.length))
            {
                cursorCue.anchoredPosition = new(timeline.GetTimelinePositionForTime(LevelEditorContext.Current.SongAudio.length), cursorCue.anchoredPosition.y);
            }
        }

        private void ClampViewportEdges(bool isDragging)
        {
            if (cursorCue.position.x > scrollRect.viewport.rect.xMax)
            {
                scrollRect.ScrollToTarget(cursorCue, isDragging ? RectType.Max : RectType.Min, RectTransform.Axis.Horizontal);
            }
            else if (cursorCue.position.x < scrollRect.viewport.rect.xMin)
            {
                scrollRect.ScrollToTarget(cursorCue, isDragging ? RectType.Min : RectType.Max, RectTransform.Axis.Horizontal);
            }
        }

        protected override void OnSongUpdate(float time)
        {
            cursorCue.anchoredPosition = new(timeline.GetTimelinePositionForTime(time), cursorCue.anchoredPosition.y);
            lastSongTimePosition = time;
            UpdateBoundaries(false);
        }

        public void MoveCursor(float positionX)
        {
            cursorCue.position = new(positionX, cursorCue.position.y);
            lastSongTimePosition = timeline.GetTimeFromTimelinePosition(cursorCue.anchoredPosition.x);
        }

        public void RefreshCursor()
        {
            cursorCue.anchoredPosition = new(timeline.GetTimelinePositionForTime(lastSongTimePosition), cursorCue.anchoredPosition.y);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragAutoplay = LevelEditorPlaybackBridge.Current.IsPlaying;
            LevelEditorPlaybackBridge.Current.Pause();
        }

        public void OnDrag(PointerEventData eventData)
        {
            MoveCursor(eventData.position.x);
            UpdateBoundaries(true);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!LevelEditorPlaybackBridge.Current.IsPlaying)
            {
                LevelEditorPlaybackBridge.Current.StartPlayback(CursorTime, autoplay: false, oneFrame: true);
            }
        }

        public void OnAreaClick(float position)
        {
            MoveCursor(position);
            PlayFromPosition();
        }

        public void PlayFromPosition()
        {
            var wasPlaying = LevelEditorPlaybackBridge.Current.IsPlaying;
            LevelEditorPlaybackBridge.Current.Pause();
            LevelEditorPlaybackBridge.Current.StartPlayback(CursorTime, autoplay: wasPlaying, oneFrame: !wasPlaying);
        }
    }
}
