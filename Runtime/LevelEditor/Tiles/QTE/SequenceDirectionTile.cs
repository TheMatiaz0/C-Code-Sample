using System;
using Telegraphist.Gameplay.QTE;
using Telegraphist.Helpers;
using Telegraphist.Helpers.Provider;
using Telegraphist.Input;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using UniRx;

namespace Telegraphist.Gameplay.Tiles
{
    [TileDefinition("Sequence Direction", ColorHtml = "#ff322b"), Serializable]
    public record SequenceDirectionTile(
        QteDirection KeyType = QteDirection.Up,
        float PreIndicationBeats = 1
        ) : Tile;

    public class SequenceDirectionTileBehaviour : SimplePressableTileBehaviour<SequenceDirectionTile>
    {
        public new enum PressStage
        {
            PreIndication,
            None,
            Pressing,
            Done
        }

        public override PressableTilesBlockMode BlockMode => PressableTilesBlockMode.None;
        protected override bool IsHoldable => false;
        private QteDirection CorrectKeyType => Tile.KeyType;
        private float PreIndicationBeats => Tile.PreIndicationBeats;

        public new PressStage Stage { get; private set; } = PressStage.PreIndication;
        private float BeatsToTileStart => Tile.StartBeat - CurrentBeat;
        public override float OverrideStartBeat => BaseStartBeat - PreIndicationBeats;

        private IQteLogic selectedQteLogic;
        private IQteContent selectedQteContent;

        public SequenceDirectionTileBehaviour(SequenceDirectionTile tile, int index, IProvider provider) : base(tile, index)
        {
            Stage = PressStage.PreIndication;
        }

        public void PopulateQte(IQteLogic logic, IQteContent content)
        {
            selectedQteLogic = logic;
            selectedQteContent = content;
        }

        protected override void OnTileStart()
        {
            if (Tile.KeyType == QteDirection.None)
            {
                return;
            }

            GameInputHandler.Current.OnQteKeyPressStarted
                           .Where(x => Stage == PressStage.None && x == CorrectKeyType)
                           .Subscribe(_ => PressStart())
                           .AddTo(Disposables);
            
            selectedQteLogic?.AddKeyObject(Tile.KeyType, TempoUtils.BeatToTime(BeatsToTileStart));
            selectedQteContent?.OnDirectionEnter(Tile.KeyType);
        }

        protected override void OnTileStay()
        {
            if (Stage == PressStage.PreIndication && CurrentBeat >= base.OverrideStartBeat)
            {
                Stage = PressStage.None;
            }
        }

        protected override void OnPressStart(AccuracyStatus accuracyStatus, float diff)
        {
            selectedQteLogic?.KeyPressed(accuracyStatus);
            selectedQteContent?.OnDirectionPress(accuracyStatus);
            PublishStatus(new QteInputStatus(accuracyStatus));
        }

        protected override void OnMiss()
        {
            selectedQteLogic?.KeyPressed(AccuracyStatus.Invalid);
            selectedQteContent?.OnDirectionPress(AccuracyStatus.Invalid);
            PublishStatus(new QteInputStatus(AccuracyStatus.Invalid));
        }

        private void PublishStatus(QteInputStatus status)
        {
            MessageBroker.Default.Publish(status);
        }

        protected override void OnPressEnd(AccuracyStatus accuracy, float diff)
        {
        }
    }
}