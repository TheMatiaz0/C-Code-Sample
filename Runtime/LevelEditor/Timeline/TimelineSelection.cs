using System.Collections.Generic;
using Telegraphist.LevelEditor.Timeline.Tiles;
using UnityEngine;

namespace Telegraphist.LevelEditor.Timeline
{
    public class TimelineSelection : MonoBehaviour
    {
        public HashSet<TimelineTile> SelectedTiles { get; private set; } = new();
        public HashSet<TimelineTile> PreviousSelectedTiles { get; private set; } = new();

        private Vector2 selectionStartPosition;
        
        public bool Contains(TimelineTile tile) => SelectedTiles.Contains(tile);
        
        public void AddToSelection(TimelineTile tile)
        {
            SelectedTiles.Add(tile);
            tile.EnableBorder(true);
        }
        
        public void RemoveFromSelection(TimelineTile tile)
        {
            SelectedTiles.Remove(tile);
            tile.EnableBorder(false);
        }
        
        public void ClearSelection()
        {
            foreach (var tile in SelectedTiles)
            {
                tile.EnableBorder(false);
            }
            SelectedTiles.Clear();
        }

        public void ApplySelection()
        {
            PreviousSelectedTiles = new HashSet<TimelineTile>(SelectedTiles);
        }
    }
}