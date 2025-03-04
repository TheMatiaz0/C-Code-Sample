using Telegraphist.LevelEditor.Timeline.Tiles;
using Telegraphist.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Telegraphist.LevelEditor.Timeline.Tools
{
    public class EditTool : MonoBehaviour, ITimelineTool
    {
        [SerializeField] private Timeline timeline;


        private Vector2Int dragStartPosition;
        private TimelineTile addingTile;

        // private bool isRemoveDragging;
        private TileTransformHelper tileTransformHelper;

        #region Events

        public void OnPointerDown(PointerEventData e, Vector2Int timelinePosition)
        {
            if (e.button == PointerEventData.InputButton.Left)
            {
                AddStart(timelinePosition);
            }
            /*
            else if (e.button == PointerEventData.InputButton.Right)
            {
                isRemoveDragging = true;
            }
            */
        }

        public void OnPointerUp(PointerEventData e, Vector2Int timelinePosition)
        {
            // isRemoveDragging = false;
            AddEnd();
        }

        public void OnDrag(PointerEventData e, Vector2Int timelinePosition)
        {
            if (e.button == PointerEventData.InputButton.Left)
            {
                AddDrag(timelinePosition);
            }
        }

        public void OnTilePointerDown(PointerEventData e, TimelineTile tile)
        {
            if (e.button == PointerEventData.InputButton.Left)
            {
                timeline.InspectTile(tile.TileBuilder.tileGuid);
            }
            else if (e.button == PointerEventData.InputButton.Right)
            {
                timeline.RemoveTile(tile.TileBuilder.tileGuid);
            }
        }

        /*
        public void OnTilePointerUp(PointerEventData e, TimelineTile tile)
        {
            isRemoveDragging = false;
        }

        public void OnTilePointerEnter(PointerEventData e, TimelineTile tile)
        {
            if (isRemoveDragging)
            {
                timeline.RemoveTile(tile.TileBuilder.tileGuid);
            }
        }
        */

        public void OnTilePointerExit(PointerEventData e, TimelineTile tile) { }

        public void OnTileBeginDrag(PointerEventData e, TimelineTile tile)
        {
            tileTransformHelper = new TileTransformHelper(timeline, tile);
            tileTransformHelper.MoveBegin(e);
        }

        public void OnTileDrag(PointerEventData e, TimelineTile tile) => tileTransformHelper.MoveDrag(e);

        public void OnTileEndDrag(PointerEventData e, TimelineTile tile) => tileTransformHelper.Apply();

        public void OnTileHandleBeginDrag(PointerEventData e, TimelineTile tile, HandleSide side)
        {
            tileTransformHelper = new TileTransformHelper(timeline, tile);
            tileTransformHelper.ResizeBegin(e);
        }

        public void OnTileHandleDrag(PointerEventData e, TimelineTile tile, HandleSide side) => tileTransformHelper.ResizeDrag(e, side);
        public void OnTileHandleEndDrag(PointerEventData e, TimelineTile tile, HandleSide side) => tileTransformHelper.Apply();

        #endregion

        private void AddStart(Vector2Int timelinePosition)
        {
            dragStartPosition = Vector2Int.FloorToInt(timelinePosition);

            addingTile = timeline.SpawnTimelineTile(new TimelineTileBuilder
            {
                column = dragStartPosition.x,
                row = dragStartPosition.y,
                width = 1,
            }.Build(timeline.BeatFraction, timeline.LastTileType));
            addingTile.EnableRaycast(false);
        }

        private void AddDrag(Vector2Int timelinePosition)
        {
            var position = Vector2Int.FloorToInt(timelinePosition);

            addingTile.TileBuilder = addingTile.TileBuilder.CopyWith((ref TimelineTileBuilder val) =>
            {
                val.column = Mathf.Min(dragStartPosition.x, position.x);
                val.width = Mathf.Max(Mathf.Abs(dragStartPosition.x - position.x) + 1, 1);
            });

            if (addingTile.Rt)
            {
                timeline.SetTilePosition(addingTile.Rt, addingTile.TileBuilder);
            }
        }

        private void AddEnd()
        {
            if (addingTile == null) return;

            timeline.AddTile(addingTile);
            addingTile = null;
        }
    }
}