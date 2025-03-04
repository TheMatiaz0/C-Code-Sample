using Telegraphist.LevelEditor.Timeline.Tiles;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Telegraphist.LevelEditor.Timeline
{
    public interface ITimelineTool
    {
        void OnToolActivate() { }
        void OnToolDeactivate() { }

        void OnBeginDrag(PointerEventData e, Vector2Int timelinePosition) { }
        void OnDrag(PointerEventData e, Vector2Int timelinePosition) { }
        void OnEndDrag(PointerEventData e, Vector2Int timelinePosition) { }
        void OnPointerClick(PointerEventData e, Vector2Int timelinePosition) { }
        void OnPointerDown(PointerEventData e, Vector2Int timelinePosition) { }
        void OnPointerUp(PointerEventData e, Vector2Int timelinePosition) { }
        
        void OnTilePointerClick(PointerEventData e, TimelineTile tile) { }
        void OnTilePointerDown(PointerEventData e, TimelineTile tile) { }
        void OnTilePointerUp(PointerEventData e, TimelineTile tile) { }
        void OnTilePointerEnter(PointerEventData e, TimelineTile tile) { }
        void OnTilePointerExit(PointerEventData e, TimelineTile tile) { }
        void OnTileBeginDrag(PointerEventData e, TimelineTile tile) { }
        void OnTileDrag(PointerEventData e, TimelineTile tile) { }
        void OnTileEndDrag(PointerEventData e, TimelineTile tile) { }

        void OnTileHandleBeginDrag(PointerEventData e, TimelineTile timelineTile, HandleSide side) { }

        void OnTileHandleDrag(PointerEventData e, TimelineTile timelineTile, HandleSide side) { }
        void OnTileHandleEndDrag(PointerEventData e, TimelineTile timelineTile, HandleSide side) { }
    }

    /// <summary>
    /// Empty implementation to use instead of null.
    /// </summary>
    public class NoTimelineTool : ITimelineTool { }
}