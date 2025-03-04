using System;
using System.Collections.Generic;
using Telegraphist.Events;
using Telegraphist.Gameplay.QTE;
using Telegraphist.Helpers.Provider;
using Telegraphist.Scriptables;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using Telegraphist.VFX;
using UniRx;

namespace Telegraphist.Gameplay.Tiles
{
    [TileDefinition("Curve Sequence Parent", ColorHtml = "#a80000"), Serializable]
    public record CurveSequenceParentTile(
        QteContent Content = QteContent.BrokenTelegraph,
        FocusObject FocusObject = FocusObject.Paper,
        float PreIndicationBeats = 2f,
        bool UseContinuousPosition = false
    ) : Tile;


    public class CurveSequenceParentTileBehaviour : TileBehaviour<CurveSequenceParentTile>
    {
        private enum Stage
        {
            PreIndication,
            Active,
        }

        public override float OverrideStartBeat => base.OverrideStartBeat - Tile.PreIndicationBeats;
        public QteCurveSequenceDisplay SequenceDisplay { get; private set; }

        private readonly IQteContent selectedQteContent;
        private readonly BalanceScriptable balanceScriptable;
        private readonly GameplayElementPivot pivot;
        private readonly LightController lightController;
        private Stage stage = Stage.PreIndication;

        public CurveSequenceParentTileBehaviour(CurveSequenceParentTile tile, int index, IProvider provider) : base(tile, index)
        {
            SequenceDisplay = provider.Get<QteCurveSequenceDisplay>();

            var qteContents = provider.GetAll<IQteContent>();
            selectedQteContent = qteContents.Find(x => x.ContentType == tile.Content);

            lightController = provider.Get<LightController>();

            balanceScriptable = BalanceScriptable.Current;
            pivot = GameplayElementPivotAssigner.Current.GetPivot(Tile.FocusObject);
        }

        protected override void OnTileStart()
        {
            selectedQteContent?.OnDirectorPreIndication();
        }

        protected override void OnTileStay()
        {
            base.OnTileStay();

            if (stage == Stage.PreIndication && CurrentBeat >= base.OverrideStartBeat)
            {
                stage = Stage.Active;
                ActivateSequence();
            }

            if (stage == Stage.Active)
            {
                SequenceDisplay.UpdateLogic(CurrentBeat);

                if (Tile.UseContinuousPosition)
                {
                    Follow();
                }
            }
        }

        private void ActivateSequence()
        {
            Focus();
            SequenceDisplay?.Initialize(Tile);

            selectedQteContent?.OnDirectorEnter();
            selectedQteContent?.Root.SetPositionAndRotation(pivot.ContentTransform.position, pivot.ContentTransform.rotation);

            Follow();
            MessageBroker.Default.Publish(new OnQuickTimeEventStart());
        }

        private void Follow()
        {
            SequenceDisplay?.Root.SetPositionAndRotation(pivot.DisplayTransform.position, pivot.DisplayTransform.rotation);
        }

        private void Focus()
        {
            FXTrigger_CameraFocus.Current.FocusOnObject(Tile.FocusObject);
            lightController.FadeLight(pivot.LightTarget, shouldFadeOut: false);
        }

        protected override void OnTileEnd()
        {
            base.OnTileEnd();
            Unfocus();

            if (SequenceDisplay != null)
            {
                SequenceDisplay.FinishSequence(out float accuracyValue);
                bool isQtePassed = balanceScriptable.IsQtePassed(accuracyValue);
                selectedQteContent?.OnDirectorExit(isQtePassed);
                MessageBroker.Default.Publish(new OnQuickTimeEventEnd(isQtePassed));
            }
        }

        private void Unfocus()
        {
            FXTrigger_CameraFocus.Current.ResetFocus();
            lightController.FadeLight(pivot.LightTarget, shouldFadeOut: true);
        }
    }
}
