using System;
using Telegraphist.Events;
using Telegraphist.Input;
using Telegraphist.TileSystem;
using UniRx;

namespace Telegraphist.Gameplay.Tiles
{
    public abstract class SmashableTileBehaviour<T> : TileBehaviour<T>, IPressableTileBehaviour where T : Tile
    {
        public const float ThrottleSeconds = 0.01f;
        
        public bool CanBePressed => true;
        PressableTilesBlockMode IBlockPressableTiles.BlockMode => PressableTilesBlockMode.ImmediateEnd;
        
        protected int SmashCount { get; private set; }

        protected SmashableTileBehaviour(T tile, int index) : base(tile, index) { }

        protected override void OnTileStart()
        {
            MessageBroker.Default.Publish(new OnQuickTimeEventStart());
            SmashCount = 0;
            GameInputHandler.Current.OnPressStarted
                .Throttle(TimeSpan.FromSeconds(ThrottleSeconds)) // https://reactivex.io/documentation/operators/debounce.html
                .Subscribe(_ => OnPress())
                .AddTo(Disposables);
        }

        protected override void OnTileStay() { }

        protected override void OnTileEnd() { }

        private void OnPress()
        {
            SmashCount++;
            OnSmash();
        }

        protected abstract void OnSmash();
    }
}
