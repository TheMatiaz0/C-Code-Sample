using Cysharp.Threading.Tasks;
using System;
using Telegraphist.Events;
using Telegraphist.Gameplay.QTE.PrecisionZone;
using Telegraphist.Helpers.Provider;
using Telegraphist.Input;
using Telegraphist.TileSystem;
using UniRx;

namespace Telegraphist.Gameplay.Tiles
{
    public abstract class AbstractPrecisionTileBehaviour<T> : TileBehaviour<T> where T : PrecisionZoneTile
    {
        private readonly PrecisionZoneController precisionZoneController;

        protected override bool IsManualDispose => true;
        private GameInputHandler Input => GameInputHandler.Current;

        public AbstractPrecisionTileBehaviour(T tile, int index, IProvider provider) : base(tile, index)
        {
            precisionZoneController = provider.Get<PrecisionZoneController>();
        }

        protected override void OnTileStart()
        {
            MessageBroker.Default.Publish(new OnQuickTimeEventStart());
            PrecisionStartAsync().Forget();
        }

        protected override void OnTileStay()
        {
        }

        protected override void OnTileEnd()
        {
        }

        protected virtual void DisplayInitialization()
        {
            precisionZoneController.IndicateInitialization();
        }

        protected virtual void DisplaySuccessResult()
        {
            MessageBroker.Default.Publish(new OnQuickTimeEventEnd(IsSuccess: true));
            precisionZoneController.Success();
        }

        protected virtual void DisplayTimeRunOutResult()
        {
            base.Dispose();
            DisplayFailResult();
            PrecisionEndAsync().Forget();
        }

        protected virtual void DisplayFailResult()
        {
            MessageBroker.Default.Publish(new OnQuickTimeEventEnd(IsSuccess: false));
            precisionZoneController.Fail();
        }

        private void OnPress(Unit unit)
        {
            base.Dispose();
            bool isPressSuccesful = precisionZoneController.TryGetSuccessPress();
            if (isPressSuccesful)
            {
                DisplaySuccessResult();
            }
            else
            {
                DisplayFailResult();
            }
            PrecisionEndAsync().Forget();
        }

        private async UniTask PrecisionStartAsync()
        {
            precisionZoneController.Initialize(Tile);
            DisplayInitialization();
            await UniTask.Delay(TimeSpan.FromSeconds(Tile.DelayAppearAndDisappear));
            Input.OnPressStarted.Subscribe(OnPress).AddTo(Disposables);
            precisionZoneController.StartAnimation(animationCompleteCallback: DisplayTimeRunOutResult);
        }

        private async UniTask PrecisionEndAsync()
        {
            precisionZoneController.KillAnimation();
            await UniTask.Delay(TimeSpan.FromSeconds(Tile.DelayAppearAndDisappear));
            precisionZoneController.Hide();
        }
    }
}
