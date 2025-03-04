using System;
using Telegraphist.Gameplay;
using Telegraphist.LevelEditor.Playback;
using Telegraphist.Lifecycle;
using Telegraphist.Structures;
using Telegraphist.UI.Menus;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Telegraphist.Input
{
    public class GameInputHandler : SceneSingleton<GameInputHandler>
    {
        // Telegraph tap press state
        public event Action<bool> OnPressStateChanged = delegate { };
        private Subject<bool> PressStateChanged { get; } = new();
        public IObservable<Unit> OnPressStarted => PressStateChanged.Where(x => x).AsUnitObservable();
        public IObservable<Unit> OnPressEnded => PressStateChanged.Where(x => !x).AsUnitObservable();
        private Subject<QteDirection> QteKeyPressStateChanged { get; } = new();
        public Subject<QteDirection> OnQteKeyPressStateChanged => QteKeyPressStateChanged;
        public IObservable<QteDirection> OnQteKeyPressStarted => QteKeyPressStateChanged.Where(x => x != QteDirection.None);
        public IObservable<QteDirection> OnQteKeyPressEnded => QteKeyPressStateChanged.Where(x => x == QteDirection.None);

        // Knob rotation
        private Subject<Vector2> KnobRotated { get; } = new();
        public IObservable<Vector2> OnKnobRotated => KnobRotated.AsObservable();

        private QteDirection currentDirection = QteDirection.None;

        public void OnTapTelegraph(InputValue inputValue)
        {
            var value = inputValue.Get<float>();
            var isPressed = value != 0;
            OnPressStateChanged(isPressed);
            PressStateChanged.OnNext(isPressed);
        }

        public void OnTapKey(InputValue inputValue, QteDirection keyState)
        {
            var value = inputValue.Get<float>();
            var isPressed = value != 0;

            if (isPressed)
            {
                currentDirection = keyState;
            }
            else
            {
                if (currentDirection == keyState)
                {
                    currentDirection = QteDirection.None;
                }
            }
            
            QteKeyPressStateChanged.OnNext(currentDirection);
        }

        public void SimulateQteInputPress(QteDirection keyState)
        {
            QteKeyPressStateChanged.OnNext(keyState);
        }

        public void SimulateInputHold(bool pressed)
        {
            OnPressStateChanged(pressed);
            PressStateChanged.OnNext(pressed);
        }

        public void OnRestart()
        {
            if (SongController.Current != null)
            {
                SongController.Current.RestartWithDelay();
            }
        }

        public void OnPause()
        {
            if (LevelEditorPlaybackBridge.Current)
            {
                LevelEditorPlaybackBridge.Current.SetFullscreen(false);
            }
            else if (PauseMenuController.Current)
            {
                PauseMenuController.Current.TryOpenPause();
            }
        }

        public void OnKnobRotate(InputValue input)
        {
            var position = input.Get<Vector2>();
            KnobRotated.OnNext(position);
        }
    }
}
