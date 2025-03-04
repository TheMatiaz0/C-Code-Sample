using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Telegraphist.Helpers;
using Telegraphist.LevelEditor.Timeline.Tiles;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Telegraphist.LevelEditor.Timeline.Tools
{
    public class SelectionTool : MonoBehaviour, ITimelineTool
    {
        [SerializeField] private Timeline timeline;
        [SerializeField] private InputActionReference shiftAction;
        [SerializeField] private RectTransform selectionBox;
        [SerializeField] private TimelineSelection selection;
        
        private Vector2 selectionStartPosition;
        private TileTransformHelper tileTransformHelper;
        
        private bool IsAppendMode => shiftAction.action.ReadValue<float>() > 0;

        private void Awake()
        {
            selectionBox.sizeDelta = Vector2.zero;
        }

        public void OnToolActivate()
        {
        }

        public void OnToolDeactivate()
        {
            selection.ClearSelection();
        }

        #region Events

        public void OnPointerClick(PointerEventData e, Vector2Int timelinePosition)
        {
            if (e.button == PointerEventData.InputButton.Left && !e.dragging && tileTransformHelper?.IsBusy != true)
            {
                selection.ClearSelection();
            }
        }

        public void OnBeginDrag(PointerEventData e, Vector2Int timelinePosition) => SelectionBoxBeginDrag(e, timelinePosition);

        public void OnDrag(PointerEventData e, Vector2Int timelinePosition) => SelectionBoxDrag(e, timelinePosition);

        public void OnEndDrag(PointerEventData e, Vector2Int timelinePosition) => SelectionBoxEndDrag(e, timelinePosition);

        public void OnTilePointerClick(PointerEventData e, TimelineTile tile)
        {
            if (e.button == PointerEventData.InputButton.Left && !e.dragging)
            {
                SelectByClick(e, tile);
            }
            else if (e.button == PointerEventData.InputButton.Right)
            {
                selection.AddToSelection(tile);
                RemoveAllSelectedTiles();
            }
        }

        public void RemoveAllSelectedTiles()
        {
            timeline.RemoveTiles(selection.SelectedTiles.ToList());
        }

        public void OnTileBeginDrag(PointerEventData e, TimelineTile tile)
        {
            selection.AddToSelection(tile);
            tileTransformHelper = new TileTransformHelper(timeline, tile, selection.SelectedTiles.ToList());
            tileTransformHelper.MoveBegin(e);
        }

        public void OnTileDrag(PointerEventData e, TimelineTile tile) => tileTransformHelper.MoveDrag(e);

        public void OnTileEndDrag(PointerEventData e, TimelineTile tile) => tileTransformHelper.Apply();
        
        public void OnTileHandleBeginDrag(PointerEventData e, TimelineTile tile, HandleSide side)
        {
            selection.AddToSelection(tile);
            tileTransformHelper = new TileTransformHelper(timeline, tile, selection.SelectedTiles.ToList());
            tileTransformHelper.ResizeBegin(e);
        }
        
        public void OnTileHandleDrag(PointerEventData e, TimelineTile tile, HandleSide side) => tileTransformHelper.ResizeDrag(e, side);
        public void OnTileHandleEndDrag(PointerEventData e, TimelineTile tile, HandleSide side) => tileTransformHelper.Apply();
        #endregion
        
        private void SelectByClick(PointerEventData e, TimelineTile tile)
        {
            if (IsAppendMode)
            {
                if (selection.Contains(tile)) selection.RemoveFromSelection(tile);
                else selection.AddToSelection(tile);
            }
            else
            {
                selection.ClearSelection();
                selection.AddToSelection(tile);
                timeline.InspectTile(tile.TileBuilder.tileGuid);
            }
        }
        
        private void SelectionBoxBeginDrag(PointerEventData e, Vector2Int timelinePosition)
        {
            if (IsAppendMode)
            {
                selection.ApplySelection();
            }
            else
            {
                selection.PreviousSelectedTiles.Clear();
                selection.ClearSelection();
            }
            
            var position = timeline.PointerEventToPosition(e);
            selectionStartPosition = position;
            selectionBox.anchoredPosition = position;
            selectionBox.sizeDelta = Vector2.zero;
        }
        
        private void SelectionBoxDrag(PointerEventData e, Vector2Int timelinePosition)
        {
            var position = timeline.PointerEventToPosition(e);
            DrawSelectionBox(position);
            FindSelectedTiles();
        }
        
        private void SelectionBoxEndDrag(PointerEventData e, Vector2Int timelinePosition)
        {
            selectionBox.sizeDelta = Vector2.zero;
        }

        private void DrawSelectionBox(Vector2 endPosition)
        {
            var size = endPosition - selectionStartPosition;
            
            if (size.x < 0)
            {
                size.x *= -1;
                selectionBox.pivot = new Vector2(1, selectionBox.pivot.y);
            }
            else
            {
                selectionBox.pivot = new Vector2(0, selectionBox.pivot.y);
            }
            
            if (size.y < 0)
            {
                size.y *= -1;
                selectionBox.pivot = new Vector2(selectionBox.pivot.x, 1);
            }
            else
            {
                selectionBox.pivot = new Vector2(selectionBox.pivot.x, 0);
            }
            
            selectionBox.sizeDelta = size;
        }
        
        private void FindSelectedTiles()
        {
            var rect = new Rect(selectionBox.offsetMin, selectionBox.sizeDelta);
            foreach (var tile in timeline.TimelineTiles.Values)
            {
                var tileRect = new Rect(tile.Rt.offsetMin, tile.Rt.sizeDelta);
                if (rect.Overlaps(tileRect, true) || (IsAppendMode && selection.PreviousSelectedTiles.Contains(tile)))
                {
                    selection.AddToSelection(tile);
                }
                else
                {
                    selection.RemoveFromSelection(tile);
                }
            }
        }
        
    }
}