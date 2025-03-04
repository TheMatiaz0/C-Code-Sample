using System;
using Telegraphist.Events;
using Telegraphist.Gameplay.QTE;
using Telegraphist.TileSystem;
using UniRx;
using Telegraphist.Gameplay.QTE.Frequency;
using Telegraphist.Helpers.Provider;
using Telegraphist.VFX;
using Telegraphist.Structures;
using UnityEngine;

namespace Telegraphist.Gameplay.Tiles
{
    [TileDefinition("Frequency Change", ColorHtml = "#fc538f"), Serializable]
    public record FrequencyChangeTile(
        int TargetFrequency = 0,
        bool UseRandomFrequency = true,
        float PreIndicationBeats = 0.5f,
        float PostIndicationBeats = 0.5f
    ) : Tile;

    public class FrequencyChangeTileBehaviour : TileBehaviour<FrequencyChangeTile>, IBlockPressableTiles
    {
        private enum Stage
        {
            PreIndication,
            Input,
            PostIndication,
            Done
        }
        
        PressableTilesBlockMode IBlockPressableTiles.BlockMode => PressableTilesBlockMode.Ignore;
        private Vector2 Offset => new(-1.4f, 0.8f);
        
        public override float OverrideStartBeat => base.OverrideStartBeat - Tile.PreIndicationBeats;
        public override float OverrideEndBeat => base.OverrideEndBeat + Tile.PostIndicationBeats;
        public override float OverrideDurationBeats => OverrideEndBeat - OverrideStartBeat;

        private Stage stage = Stage.PreIndication;
        public readonly int TargetFrequency;
        
        private readonly KnobInputController knob;
        private readonly FrequencySwitcher frequencySwitcher;
        private readonly FrequencyController frequencyController;
        private readonly FrequencyKnobArrows frequencyKnobArrows;
        private readonly LightController lightController;
        private readonly GameplayElementPivot pivot;

        public FrequencyChangeTileBehaviour(FrequencyChangeTile tile, int index, IProvider provider) : base(tile, index)
        {
            knob = provider.Get<KnobInputController>();
            frequencySwitcher = provider.Get<FrequencySwitcher>();
            frequencyKnobArrows = provider.Get<FrequencyKnobArrows>();
            lightController = provider.Get<LightController>();

            frequencyController = FrequencyController.Current;

            TargetFrequency = Tile.UseRandomFrequency ? frequencyController.GetNextRandomFrequency() : Tile.TargetFrequency;
            frequencyController.RegisterFrequencyTile(TargetFrequency);
            
            pivot = GameplayElementPivotAssigner.Current.GetPivot(FocusObject.FrequencySwitcher);
        }
        
        protected override void OnTileStart()
        {
            MessageBroker.Default.Publish(new OnQuickTimeEventStart());
            PreIndication();
            frequencySwitcher.Initialize();
            Follow();
        }

        private void Follow()
        {
            Vector2 offsetPosition = new(pivot.ContentTransform.position.x + Offset.x, pivot.ContentTransform.position.y + Offset.y);
            frequencySwitcher?.Root.SetPositionAndRotation(offsetPosition, pivot.ContentTransform.rotation);
        }

        protected override void OnTileStay()
        {
            ChangeStage();
        }

        protected override void OnTileEnd()
        {
            CheckGameOver();
            knob.AllowRotation = false;
        }

        private void ChangeStage()
        {
            var beatsFromStart = CurrentBeat - Tile.StartBeat;
            if (stage == Stage.PreIndication && beatsFromStart > Tile.PreIndicationBeats)
            {
                stage = Stage.Input;
                BeginInput();
            }

            var beatsUntilEnd = Tile.EndBeat - CurrentBeat;
            if (stage == Stage.Input && beatsUntilEnd < Tile.PostIndicationBeats)
            {
                stage = Stage.PostIndication;
                PostIndication();
            }
        }

        private void PreIndication()
        {
            frequencySwitcher.StartBlinking(TargetFrequency);
            Focus();
        }

        private void BeginInput()
        {
            knob.AllowRotation = true;
            knob.RotationsFromZero = frequencyController.Frequency;
            knob.OnRotationsChangeFromZeroSnapped
                .Subscribe(OnRotationsChange)
                .AddTo(Disposables);

            if (TargetFrequency > knob.RotationsFromZero)
            {
                frequencyKnobArrows.ShowLeftArrow();
            }
            else
            {
                frequencyKnobArrows.ShowRightArrow();
            }
        }

        private void OnRotationsChange(int rotations)
        {
            if (rotations == TargetFrequency && stage != Stage.Done)
            {
                stage = Stage.Done;
                Cleanup(true);
            }
        }


        private void PostIndication()
        {
        }

        private void CheckGameOver()
        {
            if (stage != Stage.Done)
            {
                stage = Stage.Done;
                Cleanup(false);
            }
        }

        private void Cleanup(bool success)
        {
            knob.RotationsFromZero = TargetFrequency;
            FrequencyController.Current.Frequency = TargetFrequency;
            
            MessageBroker.Default.Publish(new OnFrequencyChangeTileEnd(TargetFrequency, TileIndex, success));
            MessageBroker.Default.Publish(new OnQuickTimeEventEnd(success));

            knob.AllowRotation = false;
            frequencyKnobArrows.HideArrows();
            frequencySwitcher.Complete(success);

            Unfocus();

            Dispose();
        }

        private void Focus()
        {
            FXTrigger_CameraFocus.Current.FocusOnObject(FocusObject.FrequencySwitcher);
            lightController.FadeLight(pivot.LightTarget, shouldFadeOut: false);

        }

        private void Unfocus()
        {
            lightController.FadeLight(pivot.LightTarget, shouldFadeOut: true);
            FXTrigger_CameraFocus.Current.ResetFocus();
        }
    }
}
