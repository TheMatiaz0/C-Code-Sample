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
using UnityEngine;

namespace Telegraphist.Gameplay.Tiles
{
    [TileDefinition("Sequence Director", ColorHtml = "#a80000"), Serializable]
    public record SequenceDirectorTile(
        QteContent Content = QteContent.BrokenTelegraph,
        FocusObject FocusObject = FocusObject.Paper, 
        QteLayout Layout = QteLayout.Both,
        float PreIndicationBeats = 2f,
        bool UseContinuousPosition = false
        ) : Tile;

    public class SequenceDirectorTileBehaviour : TileBehaviour<SequenceDirectorTile>
    {
        private enum Stage
        {
            PreIndication,
            Active,
        }
        
        public override float OverrideStartBeat => base.OverrideStartBeat - Tile.PreIndicationBeats;
        private QteLogic Logic => QteLogic.ArrowSequence;

        private readonly IQteLogic selectedQteLogic;
        private readonly IQteContent selectedQteContent;
        private readonly TileController tileController;
        private readonly BalanceScriptable balanceScriptable;
        private readonly GameplayElementPivot pivot;
        private readonly LightController lightController;
        private List<TileBehaviour> tileChildren; 
        private Stage stage = Stage.PreIndication;

        public SequenceDirectorTileBehaviour(SequenceDirectorTile tile, int index, IProvider provider) : base(tile, index)
        {
            var qteLogistics = provider.GetAll<IQteLogic>();
            selectedQteLogic = qteLogistics.Find(x => x.LogicType == Logic);

            var qteContents = provider.GetAll<IQteContent>();
            selectedQteContent = qteContents.Find(x => x.ContentType == tile.Content);

            lightController = provider.Get<LightController>();

            tileController = TileController.Current;
            balanceScriptable = BalanceScriptable.Current;
            pivot = GameplayElementPivotAssigner.Current.GetPivot(Tile.FocusObject);
        }

        protected override void OnTileStart()
        {
            selectedQteContent?.OnDirectorPreIndication();
            
            if (Tile.Layout == QteLayout.None)
            {
                Debug.Log("No tile layout specified, abandoning tile start");
                return;
            }

            tileChildren = tileController.GetChildTiles(Tile);

            foreach (var item in tileChildren)
            {
                if (item is SequenceDirectionTileBehaviour sequenceDirection)
                {
                    sequenceDirection.PopulateQte(selectedQteLogic, selectedQteContent);
                }
            }
        }

        protected override void OnTileStay()
        {
            base.OnTileStay();
            if (stage == Stage.PreIndication && CurrentBeat >= base.OverrideStartBeat)
            {
                stage = Stage.Active;
                ActivateSequence();
            }
            if (stage == Stage.Active && Tile.UseContinuousPosition)
            {
                Follow();
            }
        }
        
        private void ActivateSequence()
        {
            Focus();

            int requiredPresses = tileChildren.Count;
            selectedQteLogic.Initialize(Tile.Layout, requiredPresses);
            selectedQteContent?.OnDirectorEnter();
            selectedQteContent?.Root.SetPositionAndRotation(pivot.ContentTransform.position, pivot.ContentTransform.rotation);
            
            Follow();
            MessageBroker.Default.Publish(new OnQuickTimeEventStart());
        }

        private void Follow()
        {
            selectedQteLogic?.Root.SetPositionAndRotation(pivot.DisplayTransform.position, pivot.DisplayTransform.rotation);
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

            if (selectedQteLogic != null)
            {
                selectedQteLogic.FinishSequence(out float accuracyValue);
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