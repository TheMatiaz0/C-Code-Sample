using Telegraphist.Events;
using Telegraphist.Input;
using Telegraphist.TileSystem;
using UniRx;
using UnityEngine;

namespace Telegraphist.Gameplay.TileInput
{
    public class DummyMorseTileChecker : MonoBehaviour
    {
        private TileController TileController => TileController.Current;

        private void Start()
        {
            GameInputHandler.Current.OnPressStarted
                .Subscribe(_ => CheckDummy())
                .AddTo(this);
        }

        private void CheckDummy()
        {
            if (IsAnyPressableTileActive()) return;
            
            MessageBroker.Default.Publish<TileInputStatus>(new StatusDummy());
        }

        private bool IsAnyPressableTileActive()
        {
            foreach (var handle in TileController.ActiveTiles)
            {
                if (handle is IPressableTileBehaviour { CanBePressed: true })
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}
