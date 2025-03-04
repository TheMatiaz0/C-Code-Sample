using System;
using Telegraphist.Gameplay.TileInput;
using Telegraphist.Scriptables;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay.Tiles
{
    [TileDefinition("Morse", ColorHtml = "#e8e8e8", ShowOnLanes = true), Serializable]
    public record MorseTile : Tile, IFrequencyChangeFirstTile;

    public class MorseTileBehaviour : SimplePressableTileBehaviour<MorseTile>
    {
        public MorseTileBehaviour(MorseTile tile, int index) : base(tile, index) { }

        private bool isPressing;

        protected override void OnPressStart(AccuracyStatus accuracy, float diff)
        {
            isPressing = true;
            
            var context = PressInRangeHelper.GetPressStatusContext(diff);
            PublishStatus(new StatusPressStarted(Tile, TileIndex, accuracy, context));
        }

        protected override void OnPressEnd(AccuracyStatus accuracy, float diff)
        {
            isPressing = false;
            
            var context = PressInRangeHelper.GetPressStatusContext(diff);
            PublishStatus(new StatusPressEnded(Tile, TileIndex, accuracy, context));
        }

        protected override void OnMiss()
        {
            isPressing = false;
            
            PublishStatus(new StatusMissed(Tile, TileIndex));
        }

        protected override void OnTileStay()
        {
            base.OnTileStay();

            if (isPressing && BalanceScriptable.Current.IsTileLong(Tile))
            {
                var progress = Mathf.Clamp01((CurrentBeat - Tile.StartBeat) / Tile.Duration);
                MessageBroker.Default.Publish(new OnTileHold
                {
                    Progress = progress,
                    Tile = Tile,
                    Index = TileIndex
                });
            }
        }

        private void PublishStatus(TileInputStatus status) => MessageBroker.Default.Publish<TileInputStatus>(status);
    }
}
