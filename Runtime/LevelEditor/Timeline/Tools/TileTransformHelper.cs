using System.Collections.Generic;
using Telegraphist.LevelEditor.Timeline.Tiles;
using Telegraphist.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Telegraphist.LevelEditor.Timeline.Tools
{
    public class TileTransformHelper
    {
        private Timeline timeline;
        private TimelineTile referenceTile;
        private List<TimelineTile> selectedTiles;
        
        private Vector2Int startPointerPosition;
        private List<TimelineTileBuilder> originalTileDatas;
        
        public bool IsBusy { get; private set; }

        public TileTransformHelper(Timeline timeline, TimelineTile referenceTile, List<TimelineTile> selectedTiles = null)
        {
            this.timeline = timeline;
            this.referenceTile = referenceTile;
            this.selectedTiles = selectedTiles ?? new List<TimelineTile> { referenceTile };
            
            this.originalTileDatas = new List<TimelineTileBuilder>();
            foreach (var tile in this.selectedTiles)
            {
                originalTileDatas.Add(tile.TileBuilder);
            }
        }
        
        public void MoveBegin(PointerEventData eventData)
        {
            IsBusy = true;
            startPointerPosition = timeline.PointerEventToGridPositionInt(eventData);
        }

        public void MoveDrag(PointerEventData eventData)
        {
            var position = timeline.PointerEventToGridPositionInt(eventData);
            var delta = position - startPointerPosition;

            for (var i = 0; i < selectedTiles.Count; i++)
            {
                var tile = selectedTiles[i];
                var original = originalTileDatas[i];
                tile.TileBuilder = tile.TileBuilder.CopyWith((ref TimelineTileBuilder val) =>
                {
                    val.column = Mathf.Max(original.column + delta.x, 0);
                    val.row = Mathf.Clamp(original.row + delta.y, 0, timeline.RowCount - 1);
                });
                timeline.SetTilePosition(tile.Rt, tile.TileBuilder);
            }
        }

        public void ResizeBegin(PointerEventData eventData)
        {
            IsBusy = true;
            startPointerPosition = timeline.PointerEventToGridPositionInt(eventData);

            foreach (var tile in selectedTiles)
            {
                tile.IsResizing = true;
            }
        }

        public void ResizeDrag(PointerEventData eventData, HandleSide side)
        {
            var position = timeline.PointerEventToGridPositionInt(eventData);
            var delta = position.x - startPointerPosition.x;
            var (columnDelta, widthDelta) = side switch
            {
                HandleSide.Left => (delta, -delta),
                HandleSide.Right => (0, delta),
                _ => (0, 0)
            };

            for (var i = 0; i < selectedTiles.Count; i++)
            {
                var tile = selectedTiles[i];
                var newTile = tile.TileBuilder.CopyWith((ref TimelineTileBuilder val) =>
                {
                    val.column = Mathf.Max(originalTileDatas[i].column + columnDelta, 0);
                    val.width = originalTileDatas[i].width + widthDelta;
                });
                if (newTile.width <= 0) continue;
                
                tile.TileBuilder = newTile;
                timeline.SetTilePosition(tile.Rt, tile.TileBuilder);
            }
        }
        
        public void Apply()
        {
            IsBusy = false;
            
            var list = new List<TimelineTileBuilder>();
            foreach (var tile in selectedTiles)
            {
                list.Add(tile.TileBuilder);
                tile.IsResizing = false;
            }
            
            timeline.UpdateTiles(list);
        }
    }
}