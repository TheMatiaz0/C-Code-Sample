using Telegraphist.Gameplay.TileInput;
using Telegraphist.Helpers;
using Telegraphist.Helpers.Settings;
using Telegraphist.Input;
using Telegraphist.Scriptables;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay.Tiles
{
    public abstract class SimplePressableTileBehaviour<T> : TileBehaviour<T>, IPressableTileBehaviour
        where T : Tile
    {
        public enum PressStage
        {
            None,
            Pressing,
            Done
        }
        
        public override float OverrideStartBeat => base.OverrideStartBeat + InputDelayBeats - MaxInputOffsetBeats;
        protected float BaseStartBeat => base.OverrideStartBeat;
        public override float OverrideEndBeat => base.OverrideEndBeat + InputDelayBeats + MaxInputOffsetBeats;
        public override float OverrideDurationBeats => OverrideEndBeat - OverrideStartBeat;
        
        public PressStage Stage { get; private set; } = PressStage.None;
        public bool CanBePressed => Stage == PressStage.None;
        public virtual PressableTilesBlockMode BlockMode => CanBePressed ? PressableTilesBlockMode.Delay : PressableTilesBlockMode.None;
        public bool HasMoreTolerance { get; set; }
        
        protected virtual bool IsHoldable => BalanceScriptable.Current.IsTileLong(Tile);
        
        protected float InputDelayBeats => TempoUtils.TimeToBeat(SettingsController.Settings.InputLatency);
        protected float MaxInputOffsetBeats => TempoUtils.TimeToBeat(TimingValuesStore.GetMaxInputOffset(moreTolerance: HasMoreTolerance));
        protected float CurrentBeatWithInputDelay => CurrentBeat - InputDelayBeats;
        

        protected SimplePressableTileBehaviour(T tile, int index) : base(tile, index) { }

        protected abstract void OnPressStart(AccuracyStatus accuracy, float diff);
        protected abstract void OnPressEnd(AccuracyStatus accuracy, float diff);
        protected abstract void OnMiss();

        protected override void OnTileStart()
        {
            GameInputHandler.Current.OnPressStarted
                .Where(_ => Stage == PressStage.None)
                .Subscribe(_ => PressStart())
                .AddTo(Disposables);

            if (IsHoldable)
            {
                GameInputHandler.Current.OnPressEnded
                    .Where(_ => Stage == PressStage.Pressing)
                    .Subscribe(_ => PressEnd())
                    .AddTo(Disposables);
            }
        }

        protected override void OnTileStay()
        {
            if (!CanBePressed) return;

            var beatsFromStart = CurrentBeatWithInputDelay - Tile.StartBeat;
            if (beatsFromStart > MaxInputOffsetBeats)
            {
                Miss();
            }
        }

        protected override void OnTileEnd()
        {
            if (CanBePressed)
            {
                Miss();
            }
            else if (Stage == PressStage.Pressing && IsHoldable)
            {
                // player is holding for too long
                OnPressEnd(AccuracyStatus.Invalid, 0);
            }
        }

        protected void PressStart()
        {
            if (Stage != PressStage.None) return;

            Stage = PressStage.Pressing;
            
            var (result, diff, accuracy) = PressInRangeHelper.CheckInRange(CurrentBeatWithInputDelay, Tile.StartBeat, MaxInputOffsetBeats);
            if (result == PressInRangeResult.InRange)
            {
                var status = BalanceScriptable.Current.GetAccuracyStatus(accuracy);
                OnPressStart(status, diff);
            }
            else if (result == PressInRangeResult.TooLate)
            {
                Miss();
            }
            else
            {
                Debug.LogError(
                    $"Press in range result is {result}, this should never happen!\n" +
                    $"song = {SongController.Current.CurrentSong.Name}, current beat = {CurrentBeatWithInputDelay}, " +
                    $"tile index = {TileIndex}, tile start beat = {Tile.StartBeat}, " +
                    $"diff = {diff}, accuracy = {accuracy}, max input offset beats = {MaxInputOffsetBeats}, " +
                    $"has more tolerance = {HasMoreTolerance}");
            }
        }

        protected void PressEnd()
        {
            if (Stage != PressStage.Pressing) return;

            Stage = PressStage.Done;

            var (result, diff, accuracy) = PressInRangeHelper.CheckInRange(CurrentBeatWithInputDelay, Tile.EndBeat, MaxInputOffsetBeats);
            var status = result == PressInRangeResult.InRange
                ? BalanceScriptable.Current.GetAccuracyStatus(accuracy)
                : AccuracyStatus.Invalid;

            OnPressEnd(status, diff);
            Dispose();
        }

        protected void Miss()
        {
            if (!IsAlive) return;
            OnMiss();
            Dispose();
        }
    }
}
