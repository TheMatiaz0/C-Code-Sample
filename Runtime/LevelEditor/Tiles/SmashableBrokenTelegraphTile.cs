using System;
using Telegraphist.Events;
using Telegraphist.Gameplay.QTE;
using Telegraphist.Gameplay.QTE.BrokenTelegraph;
using Telegraphist.Helpers.Provider;
using Telegraphist.Structures;
using Telegraphist.TileSystem;
using Telegraphist.VFX;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay.Tiles
{
    [TileDefinition("Smashable", ColorHtml = "#e66363"), Serializable]
    public record SmashableTile(
        int RequiredSmashes = 5
    ) : Tile;

    public class SmashableBrokenTelegraphTileBehaviour : SmashableTileBehaviour<SmashableTile>
    {
        private readonly BrokenTelegraphController brokenTelegraphController;
        private readonly LightController lightController;
        private readonly FXTrigger_CameraFocus cameraFocus;
        private readonly GameplayElementPivot pivot;
        
        public SmashableBrokenTelegraphTileBehaviour(SmashableTile tile, int index, IProvider provider) : base(tile, index)
        {
            brokenTelegraphController = provider.Get<BrokenTelegraphController>();
            lightController = provider.Get<LightController>();
            cameraFocus = FXTrigger_CameraFocus.Current;
            pivot = GameplayElementPivotAssigner.Current.GetPivot(FocusObject.TelegraphKey);
        }

        protected override void OnTileStart()
        {
            if (Tile.RequiredSmashes <= 0)
            {
                Debug.LogError("Required smashes is set to 0!");
                return;
            }
            
            brokenTelegraphController.StartBrokenTelegraph(Tile.RequiredSmashes);
            Follow();
            Focus();
            base.OnTileStart();
        }

        private void Follow()
        {
            brokenTelegraphController?.Root.SetPositionAndRotation(pivot.ContentTransform.position, pivot.ContentTransform.rotation);
        }

        protected override void OnSmash()
        {
            brokenTelegraphController.UpdateSmashCount(SmashCount);
        
            if (SmashCount >= Tile.RequiredSmashes)
            {
                Success();
            }
        }

        protected override void OnTileEnd()
        {
            base.OnTileEnd();
            if (SmashCount < Tile.RequiredSmashes)
            {
                Fail();
            }
            Unfocus();
        }

        private void Fail()
        {
            MessageBroker.Default.Publish(new OnQuickTimeEventEnd(IsSuccess: false));
            brokenTelegraphController.BrokenTelegraphTimeout();
        }

        private void Success()
        {
            MessageBroker.Default.Publish(new OnQuickTimeEventEnd(IsSuccess: true));
            brokenTelegraphController.BrokenTelegraphSuccess();
            Unfocus();
        }

        private void Focus()
        {
            lightController.FadeLight(pivot.LightTarget, shouldFadeOut: false);
            cameraFocus.FocusOnObject(FocusObject.TelegraphKey);
        }

        private void Unfocus()
        {
            lightController.FadeLight(pivot.LightTarget, shouldFadeOut: true);
            cameraFocus.ResetFocus();
        }
    }
}