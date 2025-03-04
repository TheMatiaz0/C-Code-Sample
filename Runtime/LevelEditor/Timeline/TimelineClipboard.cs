using System;
using System.Collections.Generic;
using System.Linq;
using Telegraphist.Helpers;
using Telegraphist.LevelEditor.Timeline.Tiles;
using UniRx;
using UnityEngine;

namespace Telegraphist.LevelEditor.Timeline
{
    public class TimelineClipboard : MonoBehaviour
    {
        [SerializeField] private TimelineSelection selection;
        [SerializeField] private Timeline timeline;
        [SerializeField] private TimelineCursor timelineCursor;

        private List<TimelineTile> clipboard = new();
        
        private void Start()
        {
            LevelEditorInputReceiver.Current.OnCopied.Subscribe(_ => Copy()).AddTo(this);
            LevelEditorInputReceiver.Current.OnPasted.Subscribe(_ => Paste()).AddTo(this);
            LevelEditorInputReceiver.Current.OnDeleted.Subscribe(_ => Delete()).AddTo(this);
        }
        
        private void Copy()
        {
            clipboard = selection.SelectedTiles.ToList();
        }
        
        private void Paste()
        {
            if (clipboard.Count == 0) return;
            
            selection.ClearSelection();
            
            var cursorBeat = TempoUtils.TimeToBeat(timelineCursor.CursorTime);
            cursorBeat = TempoUtils.Snap(cursorBeat, 1f / timeline.BeatFraction, Mathf.Floor);
            
            var firstTileStartBeat = clipboard.OrderBy(x => x.TileBuilder.column)
                .First().TileBuilder.Build(timeline.BeatFraction).StartBeat;
            var offset = cursorBeat - firstTileStartBeat;
            
            foreach (var tile in clipboard)
            {
                var newTile = tile.TileBuilder.Build(timeline.BeatFraction, guid: Guid.NewGuid());
                newTile.StartBeat += offset;
                var timelineTile = timeline.AddTileImmediately(newTile);
                selection.AddToSelection(timelineTile);
            }
        }

        private void Delete()
        {
            if (selection.SelectedTiles.Count == 0) return;
            timeline.RemoveTiles(selection.SelectedTiles.ToList());
        }
    }
}