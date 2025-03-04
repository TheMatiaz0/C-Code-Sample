using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Telegraphist.Helpers;
using Telegraphist.Helpers.Scenes;
using Telegraphist.LevelEditor.Inspector;
using Telegraphist.LevelEditor.Timeline.Grid;
using Telegraphist.LevelEditor.Timeline.Tiles;
using Telegraphist.Lifecycle;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Telegraphist.LevelEditor.Timeline
{
    public class Timeline : SceneSingleton<Timeline>
    {
        [Header("Grid")]
        [SerializeField] private RectTransform grid;
        [SerializeField] private RectTransform content;
        [SerializeField] private int gridRows = 3;
        [SerializeField] private int excessColumnsCount = 10;

        [SerializeField] private GridBackground gridBackground;
        [SerializeField] private TimelineBeatNumbers timelineBeatNumbers;

        [Header("Tiles")]
        [SerializeField] private TimelineTile tilePrefab;

        [Header("Other")]
        [SerializeField] private TimelineOptionsUI timelineOptionsUI;
        [SerializeField] private TimelineToolsController timelineToolsController;
        [SerializeField] private TileInspector inspector;

        private Vector2Int gridSize = new(20, 3);
        private float cellHeight;

        private TimelineTilePool tilePool;
        private Transform tilesRoot;

        public Dictionary<Guid, TimelineTile> TimelineTiles { get; private set; }
        private LevelEditorContext Context => LevelEditorContext.Current;

        public int RowCount => gridRows;
        public int BeatFraction => timelineOptionsUI.BeatFraction.Value;
        public int CellWidth => timelineOptionsUI.CellWidth.Value;
        public string LastTileType => inspector.LastTileType;

        protected override void Awake()
        {
            base.Awake();
            tilesRoot = grid;
            tilePool = new TimelineTilePool(tilePrefab, tilesRoot);
        }

        private void Start()
        {
            UniTask.Void(async () =>
            {
                await Context.WaitForSong();
                
                Render();
                timelineOptionsUI.BeatFraction.Pairwise().Subscribe((v) => Render(v.Previous)).AddTo(this);
                timelineOptionsUI.CellWidth.Subscribe((v) => Render()).AddTo(this);

                Context.Song.Select(x => x.TilesDict)
                    .DistinctUntilChanged()
                    .Subscribe(ApplyTileUpdatesOptimized)
                    .AddTo(this);
            });
        }

        public void WrapActionAndPreserveTimelinePosition(Action action)
        {
            var lastCameraTimePosition = GetTimeFromTimelinePosition(content.anchoredPosition.x - Screen.width/2);
            
            action.Invoke();
            
            content.anchoredPosition = new Vector2(GetTimelinePositionForTime(lastCameraTimePosition) + Screen.width/2, content.anchoredPosition.y);
        }

        public void ChangeZoom(float amount)
        {
            timelineOptionsUI.ChangeZoom(amount);
        }

        public void OnBeatFractionScroll(float axis) => timelineOptionsUI.OnBeatFractionScroll(axis);

        public float GetTimelinePositionForTime(float time)
        {
            return TempoUtils.TimeToBeat(time, Context.Song.Value.Bpm) * CellWidth * BeatFraction;
        }

        public float GetTimeFromTimelinePosition(float positionX)
        {
            return TempoUtils.BeatToTime(positionX / (CellWidth * BeatFraction), Context.Song.Value.Bpm);
        }

        public void InspectTile(Guid guid)
        {
            inspector.Inspect(guid);
        }

        public TimelineTile SpawnTimelineTile(Tile tileData)
        {
            var editorTile = new TimelineTileBuilder(tileData, BeatFraction);
            var tile = tilePool.Rent();
            tile.Setup(this, timelineToolsController, editorTile);
            SetTileData(tile, tileData);

            TimelineTiles.Add(tileData.Guid, tile);

            return tile;
        }

        // TODO: this should ignore broken tiles
        public void SetTileData(TimelineTile tile, Tile newData)
        {
            var editorTile = new TimelineTileBuilder(newData, BeatFraction);
            tile.TileBuilder = editorTile;
            SetTilePosition(tile.Rt, editorTile);

            var img = tile.RawImage;
            img.color = newData.Definition.Color;
            var rect = img.uvRect;
            rect.width = editorTile.width;
            img.uvRect = rect;
        }

        public void SetTilePosition(RectTransform rt, TimelineTileBuilder tileBuilder)
        {
            rt.anchoredPosition = new Vector2(tileBuilder.column * CellWidth, (gridSize.y - 1 - tileBuilder.row) * cellHeight);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CellWidth * tileBuilder.width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellHeight);
        }

        public void AddTile(Tile tile)
        {
            Context.Song.Update((ref SongData data) =>
            {
                data.TilesDict.Add(tile.Guid, tile);
            });
            inspector.Inspect(tile.Guid);
        }

        public void AddTile(TimelineTile tile)
        {
            tile.EnableRaycast(true);
            AddTile(tile.TileBuilder.Build(BeatFraction, LastTileType));
        }

        public TimelineTile AddTileImmediately(Tile tile)
        {
            var spawnedTile = SpawnTimelineTile(tile);
            AddTile(spawnedTile);
            return spawnedTile;
        }

        public void RemoveTile(Guid guid)
        {
            Context.Song.Update((ref SongData value) =>
            {
                value.TilesDict.Remove(guid);
            });
        }

        public void RemoveTiles(List<TimelineTile> tiles)
        {
            Context.Song.Update((ref SongData value) =>
            {
                foreach (var tile in tiles)
                {
                    value.TilesDict.Remove(tile.TileBuilder.tileGuid);
                }
            });
        }

        public void UpdateTile(TimelineTileBuilder tileBuilder)
        {
            Context.Song.Update((ref SongData value) =>
            {
                value.TilesDict[tileBuilder.tileGuid] = tileBuilder.Build(BeatFraction);
            });
        }

        public void UpdateTiles(List<TimelineTileBuilder> tiles)
        {
            Context.Song.Update((ref SongData value) =>
            {
                foreach (var tile in tiles)
                {
                    value.TilesDict[tile.tileGuid] = tile.Build(BeatFraction);
                }
            });
        }

        public Vector2 PointerEventToPosition(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(grid, eventData.position,
                    eventData.pressEventCamera,
                    out var localPoint))
            {
                throw new Exception("Point is not in rect!");
            }

            return localPoint;
        }

        public Vector2Int PointerEventToGridPositionInt(PointerEventData eventData)
        {
            var localPoint = PointerEventToPosition(eventData);

            var row = gridSize.y - 1 - Mathf.FloorToInt((localPoint.y * gridSize.y) / grid.rect.height);
            var col = Mathf.FloorToInt((localPoint.x * gridSize.x) / grid.rect.width);

            return new Vector2Int(col, row);
        }

        private void Render(int lastBeatFraction = -1)
        {
            UpdateTimelinePositionAndSize(lastBeatFraction);

            gridBackground.DrawGrid(CellWidth, cellHeight, gridSize);

            timelineBeatNumbers.DrawBeatNumbers(gridSize, CellWidth);

            TimelineCursor.Current.RefreshCursor();

            ApplyTileUpdatesOptimized(Context.Song.Value.TilesDict);
        }

        private void UpdateTimelinePositionAndSize(int lastBeatFraction)
        {
            var gridCols = Mathf.FloorToInt(TempoUtils.TimeToBeat(Context.SongAudio.length, Context.Song.Value.Bpm) * BeatFraction) + excessColumnsCount;
            gridSize = new Vector2Int(gridCols, gridRows);
            cellHeight = grid.rect.height / gridSize.y;

            var desiredWidth = CellWidth * gridSize.x;
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                desiredWidth + grid.offsetMin.x - grid.offsetMax.x);

            // adjust camera position
            if (lastBeatFraction != -1)
            {
                var fractionDifferenceRatio = (float)BeatFraction / lastBeatFraction;
                content.position *= new Vector2(fractionDifferenceRatio, 1);
            }
        }

        private void ApplyTileUpdatesOptimized(Dictionary<Guid, Tile> tiles)
        {
            TimelineTiles ??= new Dictionary<Guid, TimelineTile>();

            var tilesCopy = new Dictionary<Guid, Tile>(tiles);

            // apply differences between old and current state
            foreach (var (guid, tile) in TimelineTiles.ToList())
            {
                if (tilesCopy.TryGetValue(guid, out var newTile))
                {
                    SetTileData(tile, newTile);
                    tilesCopy.Remove(newTile.Guid);
                }
                else
                {
                    tilePool.Return(tile);
                    TimelineTiles.Remove(guid);
                }
            }

            // spawn missing tiles
            foreach (var (_, tile) in tilesCopy)
            {
                SpawnTimelineTile(tile);
            }
        }
    }
}
