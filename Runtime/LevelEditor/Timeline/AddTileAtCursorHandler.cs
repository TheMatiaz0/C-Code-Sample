using Telegraphist.Helpers;
using Telegraphist.Helpers.Settings;
using Telegraphist.LevelEditor.Playback;
using Telegraphist.Lifecycle;
using Telegraphist.TileSystem;
using UnityEngine;

namespace Telegraphist.LevelEditor.Timeline
{
    public class AddTileAtCursorHandler : SceneSingleton<AddTileAtCursorHandler>
    {
        [SerializeField] private Timeline timeline;
        [SerializeField] private string tileType = Tile.DefaultType;
        
        public void AddTileAtCurrentPosition(float pressTime)
        {
            var endTime = LevelEditorPlaybackBridge.Current.Position - SettingsController.Settings.InputLatency;
            var startTime = endTime - pressTime;
            var snapTo = 1f / timeline.BeatFraction;
            
            var (startBeatSnapped, endBeatSnapped) = TempoUtils.SnapRange(
                TempoUtils.TimeToBeat(startTime), 
                TempoUtils.TimeToBeat(endTime), 
                snapTo, Mathf.Floor);

            var tile = new Tile(
                0,
                startBeatSnapped,
                endBeatSnapped - startBeatSnapped,
                null
            );
            
            timeline.AddTile(TileRegistry.ChangeTileType(tile, tileType));
        }
    }
}